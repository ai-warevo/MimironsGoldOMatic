--[[
  Mimiron's Gold-o-Matic — WoW 3.3.5a (Interface: 30300)
  Normative: docs/overview/SPEC.md §8–§10, docs/components/wow-addon/UI_SPEC.md UI-401–405
]]

local L = {
  PANEL_TITLE = "MGM Queue",
  PREPARE_MAIL = "Prepare Mail",
  OPEN_MAILBOX = "Open the mailbox to use the queue.",
  MAIL_SUBJECT = "Mimiron's Gold-o-Matic",
  TOAST_SENT = "Mail sent — %s",
  UPDATE_CHECK_RUNNING = "Mimiron's Gold-o-Matic: выполняется проверка обновлений...",
  UPDATE_CHECK_GUIDANCE = "Mimiron's Gold-o-Matic: если вы не увидите ответ в чате, убедитесь, что запущено Desktop-приложение.",
}

local MGM_WINNER_WHISPER_BODY =
  "Поздравляю, ты победил в розыгрыше! Дай мне своё согласие на получение награды - ответь на это сообщение одной фразой: !twgold"

local MGM_MAIL_COMPLETION_WHISPER_RU =
  "Награда отправлена тебе на почту, проверяй ящик!"

--- Queue: { payoutId, characterName, copper, state = "READY"|"PROCESSING" }
local mgmQueue = {}

--- After NotifyWinnerWhisper: expect CHAT_MSG_WHISPER !twgold from this character.
local mgmPendingAcceptance = nil -- { payoutId, characterName }

--- Armed right before SendMail on MGM-prepared mail.
local mgmArmedPayout = nil

--- Carried from SendMail hook until MAIL_SEND_SUCCESS / MAIL_FAILED.
local mgmSendContext = nil

--- Single in-flight /who for roulette (§8).
local mgmWhoPending = nil -- { spinCycleId, characterName }

local mgmPanel = nil
local mgmListText = nil
local mgmHintText = nil
local mgmPrepareBtn = nil
local mgmToast = nil

local eventFrame = CreateFrame("Frame")

local Core = assert(MimironsGoldOMaticCore, "MimironsGoldOMatic.Core.lua must load before MimironsGoldOMatic.lua")
local mgmTrim = Core.Trim
local mgmShortName = Core.ShortName
local mgmNamesEqual = Core.NamesEqual

--- SPEC §8: one line [MGM_WHO] + compact JSON (no newlines inside object).
local function mgmEmitWhoJson(spinCycleId, characterName, online)
  local line = Core.BuildWhoLogLine(spinCycleId, characterName, online)
  DEFAULT_CHAT_FRAME:AddMessage(line)
end

local function mgmParseWhoOnline(expectedName)
  return Core.FindMatchingWhoName(expectedName, function()
    return GetNumWhoResults and GetNumWhoResults() or 0
  end, GetWhoInfo)
end

local function mgmEmitUpdateCheck()
  -- Keep this tag unmodified so Desktop can detect it in WoWChatLog.txt.
  DEFAULT_CHAT_FRAME:AddMessage("[MGM_UPDATE_CHECK]")
end

local function mgmRequestUpdateCheck()
  DEFAULT_CHAT_FRAME:AddMessage(L.UPDATE_CHECK_RUNNING)
  DEFAULT_CHAT_FRAME:AddMessage(L.UPDATE_CHECK_GUIDANCE)
  mgmEmitUpdateCheck()
end

--- Desktop/EBS: /run MGM_RunWhoForSpin("<spinCycleId>","<CharacterName>")
--- Uses spinCycleId from GET /api/roulette/state → currentSpinCycleId.
function MGM_RunWhoForSpin(spinCycleId, characterName)
  if not spinCycleId or spinCycleId == "" or not characterName or characterName == "" then
    return
  end
  if mgmWhoPending then
    DEFAULT_CHAT_FRAME:AddMessage("[MGM] /who already pending; try again.")
    return
  end
  mgmWhoPending = { spinCycleId = spinCycleId, characterName = characterName }
  if RunMacroText then
    RunMacroText("/who " .. characterName)
  else
    DEFAULT_CHAT_FRAME:AddMessage("[MGM] RunMacroText unavailable.")
    mgmWhoPending = nil
  end
end

--- Desktop: /run NotifyWinnerWhisper("<payoutId>","<CharacterName>")
function NotifyWinnerWhisper(payoutId, characterName)
  if not payoutId or payoutId == "" or not characterName or characterName == "" then
    return
  end
  SendChatMessage(MGM_WINNER_WHISPER_BODY, "WHISPER", nil, characterName)
  mgmPendingAcceptance = { payoutId = payoutId, characterName = characterName }
end

--- Desktop: /run ReceiveGold("uuid:Name:copper;...")
function ReceiveGold(dataString)
  if not dataString or dataString == "" then
    return
  end
  for piece in string.gmatch(dataString, "[^;]+") do
    local row = Core.ParseGoldSegment(piece)
    if row then
      table.insert(mgmQueue, row)
    end
  end
  if mgmPanel and mgmPanel:IsShown() then
    MGM_RefreshQueuePanel()
  end
end

function MGM_ArmMailSendForPayout(payoutId, characterName)
  mgmArmedPayout = { payoutId = payoutId, characterName = characterName }
end

function MGM_ClearArmedMailSend()
  mgmArmedPayout = nil
  mgmSendContext = nil
end

function MGM_PopQueueAfterSuccessfulSend()
  table.remove(mgmQueue, 1)
  if mgmPanel and mgmPanel:IsShown() then
    MGM_RefreshQueuePanel()
  end
end

local function mgmShowToast(text)
  if not mgmToast then
    return
  end
  mgmToast.text:SetText(text)
  mgmToast:Show()
  mgmToast:SetAlpha(1)
  local t = 0
  mgmToast:SetScript("OnUpdate", function(self, elapsed)
    t = t + elapsed
    if t < 2.5 then
      return
    end
    self:SetAlpha(self:GetAlpha() - elapsed * 2)
    if self:GetAlpha() <= 0 then
      self:Hide()
      self:SetScript("OnUpdate", nil)
    end
  end)
end

function MGM_RefreshQueuePanel()
  if not mgmPanel or not mgmPrepareBtn or not mgmListText then
    return
  end
  local lines = {}
  for i, row in ipairs(mgmQueue) do
    table.insert(lines, string.format("%d. %s  [%s]", i, row.characterName, row.state or "READY"))
  end
  if #lines == 0 then
    mgmListText:SetText("(empty)")
  else
    mgmListText:SetText(table.concat(lines, "\n"))
  end
  local head = mgmQueue[1]
  local canPrep = head and MailFrame and MailFrame:IsVisible() and (head.state == "READY")
  mgmPrepareBtn:SetEnabled(canPrep and true or false)
end

local function mgmPrepareMailFromQueueHead()
  local q = mgmQueue[1]
  if not q or q.state ~= "READY" then
    return
  end
  if not SendMailNameEditBox or not SendMailSubjectEditBox then
    return
  end
  SendMailNameEditBox:SetText(q.characterName)
  SendMailSubjectEditBox:SetText(L.MAIL_SUBJECT)
  if SendMailMoney and MoneyInputFrame_SetCopper then
    MoneyInputFrame_SetCopper(SendMailMoney, q.copper)
  end
  q.state = "PROCESSING"
  MGM_ArmMailSendForPayout(q.payoutId, q.characterName)
  MGM_RefreshQueuePanel()
end

local function mgmCreatePanel()
  if mgmPanel then
    return
  end
  local p = CreateFrame("Frame", "MGMQueuePanel", UIParent)
  p:SetWidth(220)
  p:SetHeight(280)
  p:SetBackdrop({
    bgFile = "Interface\\DialogFrame\\UI-DialogBox-Background",
    edgeFile = "Interface\\DialogFrame\\UI-DialogBox-Border",
    tile = true,
    tileSize = 32,
    edgeSize = 16,
    insets = { left = 4, right = 4, top = 4, bottom = 4 },
  })
  p:SetBackdropColor(0, 0, 0, 0.85)
  p:SetMovable(true)
  p:EnableMouse(true)
  p:RegisterForDrag("LeftButton")
  p:SetScript("OnDragStart", function(self)
    self:StartMoving()
  end)
  p:SetScript("OnDragStop", function(self)
    self:StopMovingOrSizing()
  end)

  local title = p:CreateFontString(nil, "ARTWORK", "GameFontNormalLarge")
  title:SetPoint("TOP", 0, -12)
  title:SetText(L.PANEL_TITLE)

  mgmHintText = p:CreateFontString(nil, "ARTWORK", "GameFontHighlightSmall")
  mgmHintText:SetPoint("TOP", title, "BOTTOM", 0, -8)
  mgmHintText:SetWidth(200)
  mgmHintText:SetText(L.OPEN_MAILBOX)

  mgmListText = p:CreateFontString(nil, "ARTWORK", "GameFontHighlight")
  mgmListText:SetPoint("TOPLEFT", 16, -56)
  mgmListText:SetWidth(190)
  mgmListText:SetJustifyH("LEFT")

  mgmPrepareBtn = CreateFrame("Button", nil, p, "UIPanelButtonTemplate")
  mgmPrepareBtn:SetWidth(160)
  mgmPrepareBtn:SetHeight(24)
  mgmPrepareBtn:SetPoint("BOTTOM", 0, 12)
  mgmPrepareBtn:SetText(L.PREPARE_MAIL)
  mgmPrepareBtn:SetScript("OnClick", function()
    mgmPrepareMailFromQueueHead()
  end)

  mgmToast = CreateFrame("Frame", nil, UIParent)
  mgmToast:SetWidth(260)
  mgmToast:SetHeight(48)
  mgmToast:SetPoint("CENTER", 0, 120)
  mgmToast:SetBackdrop({
    bgFile = "Interface\\DialogFrame\\UI-DialogBox-Background",
    edgeFile = "Interface\\DialogFrame\\UI-DialogBox-Border",
    tile = true,
    tileSize = 32,
    edgeSize = 12,
    insets = { left = 4, right = 4, top = 4, bottom = 4 },
  })
  mgmToast:SetBackdropBorderColor(0.2, 0.7, 0.2, 1)
  mgmToast:Hide()
  mgmToast.text = mgmToast:CreateFontString(nil, "ARTWORK", "GameFontNormalLarge")
  mgmToast.text:SetPoint("CENTER")

  mgmPanel = p
  p:Hide()
end

local function mgmAnchorPanel()
  if not mgmPanel then
    return
  end
  if MailFrame and MailFrame:IsVisible() then
    mgmPanel:ClearAllPoints()
    mgmPanel:SetPoint("TOPLEFT", MailFrame, "TOPRIGHT", 8, 0)
    mgmHintText:SetText("")
  else
    mgmPanel:ClearAllPoints()
    mgmPanel:SetPoint("CENTER", UIParent, "CENTER", 220, 0)
    mgmHintText:SetText(L.OPEN_MAILBOX)
  end
end

function MGM_ShowQueuePanel()
  mgmCreatePanel()
  mgmAnchorPanel()
  mgmPanel:Show()
  MGM_RefreshQueuePanel()
end

function MGM_ToggleQueuePanel()
  mgmCreatePanel()
  if mgmPanel:IsShown() then
    mgmPanel:Hide()
  else
    MGM_ShowQueuePanel()
  end
end

hooksecurefunc("SendMail", function(recipient, _, _)
  if mgmArmedPayout and recipient and mgmNamesEqual(recipient, mgmArmedPayout.characterName) then
    mgmSendContext = mgmArmedPayout
    mgmArmedPayout = nil
  end
end)

eventFrame:RegisterEvent("MAIL_SEND_SUCCESS")
eventFrame:RegisterEvent("MAIL_FAILED")
eventFrame:RegisterEvent("MAIL_SHOW")
eventFrame:RegisterEvent("MAIL_CLOSED")
eventFrame:RegisterEvent("CHAT_MSG_WHISPER")
eventFrame:RegisterEvent("WHO_LIST_UPDATE")

eventFrame:SetScript("OnEvent", function(_, event, ...)
  if event == "MAIL_SEND_SUCCESS" then
    if mgmSendContext then
      local id = mgmSendContext.payoutId
      local name = mgmSendContext.characterName
      mgmSendContext = nil
      DEFAULT_CHAT_FRAME:AddMessage("[MGM_CONFIRM:" .. id .. "]")
      SendChatMessage(MGM_MAIL_COMPLETION_WHISPER_RU, "WHISPER", nil, name)
      mgmShowToast(string.format(L.TOAST_SENT, name))
      MGM_PopQueueAfterSuccessfulSend()
    end
  elseif event == "MAIL_FAILED" then
    mgmSendContext = nil
    if mgmQueue[1] and mgmQueue[1].state == "PROCESSING" then
      mgmQueue[1].state = "READY"
    end
    MGM_ClearArmedMailSend()
    MGM_RefreshQueuePanel()
  elseif event == "MAIL_SHOW" then
    MGM_ShowQueuePanel()
  elseif event == "MAIL_CLOSED" then
    MGM_ClearArmedMailSend()
    if mgmQueue[1] and mgmQueue[1].state == "PROCESSING" then
      mgmQueue[1].state = "READY"
    end
    MGM_RefreshQueuePanel()
  elseif event == "CHAT_MSG_WHISPER" then
    local msg, sender = select(1, ...), select(2, ...)
    if not mgmPendingAcceptance then
      return
    end
    local body = mgmTrim(msg or "")
    if string.lower(body) ~= "!twgold" then
      return
    end
    if not mgmNamesEqual(sender, mgmPendingAcceptance.characterName) then
      return
    end
    DEFAULT_CHAT_FRAME:AddMessage("[MGM_ACCEPT:" .. mgmPendingAcceptance.payoutId .. "]")
    mgmPendingAcceptance = nil
  elseif event == "WHO_LIST_UPDATE" then
    if not mgmWhoPending then
      return
    end
    local spinCycleId = mgmWhoPending.spinCycleId
    local characterName = mgmWhoPending.characterName
    mgmWhoPending = nil
    local online = mgmParseWhoOnline(characterName)
    mgmEmitWhoJson(spinCycleId, characterName, online)
  end
end)

SLASH_MGM1 = "/mgm"
SlashCmdList["MGM"] = function(msg)
  local command = string.lower(mgmTrim(msg))
  if command == "update" or command == "checkupdate" then
    mgmRequestUpdateCheck()
    return
  end
  MGM_ToggleQueuePanel()
end

do
  local mb = CreateFrame("Button", "MGMMinimapButton", Minimap)
  mb:SetWidth(24)
  mb:SetHeight(24)
  mb:SetFrameStrata("MEDIUM")
  mb:SetPoint("CENTER", Minimap, "BOTTOMLEFT", 4, 4)
  mb:SetNormalTexture("Interface\\Icons\\INV_Misc_Coin_02")
  mb:SetPushedTexture("Interface\\Icons\\INV_Misc_Coin_02")
  mb:SetHighlightTexture("Interface\\Buttons\\ButtonHilight-Square", "ADD")
  mb:RegisterForClicks("LeftButtonUp")
  mb:SetScript("OnClick", function()
    MGM_ToggleQueuePanel()
  end)
  mb:SetScript("OnEnter", function(self)
    GameTooltip:SetOwner(self, "ANCHOR_LEFT")
    GameTooltip:SetText("Mimiron's Gold-o-Matic", 1, 1, 1)
    GameTooltip:AddLine("Click: toggle queue panel", nil, nil, nil, 1)
    GameTooltip:Show()
  end)
  mb:SetScript("OnLeave", function()
    GameTooltip:Hide()
  end)
  local function mgmMinimapCombat(self)
    if UnitAffectingCombat("player") then
      self:Hide()
    else
      self:Show()
    end
  end
  mb:RegisterEvent("PLAYER_REGEN_DISABLED")
  mb:RegisterEvent("PLAYER_REGEN_ENABLED")
  mb:SetScript("OnEvent", mgmMinimapCombat)
  mgmMinimapCombat(mb)
end
