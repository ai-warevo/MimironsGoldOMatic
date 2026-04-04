--[[
  Mimiron's Gold-o-Matic — WoW 3.3.5a (Interface: 30300)
  Normative copy and behavior: docs/SPEC.md §8–§10
]]

local MGM_MAIL_COMPLETION_WHISPER_RU =
  "Награда отправлена тебе на почту, проверяй ящик!"

--- Armed right before SendMail() when recipient matches (MGM queue path).
local mgmArmedPayout = nil -- { payoutId = string, characterName = string }

--- Carried from SendMail hook until MAIL_SEND_SUCCESS / MAIL_FAILED.
local mgmSendContext = nil -- same shape as mgmArmedPayout

local frame = CreateFrame("Frame")
frame:RegisterEvent("MAIL_SEND_SUCCESS")
frame:RegisterEvent("MAIL_FAILED")
frame:SetScript("OnEvent", function(_, event)
  if event == "MAIL_SEND_SUCCESS" then
    if mgmSendContext then
      local id = mgmSendContext.payoutId
      local name = mgmSendContext.characterName
      mgmSendContext = nil
      DEFAULT_CHAT_FRAME:AddMessage("[MGM_CONFIRM:" .. id .. "]")
      SendChatMessage(MGM_MAIL_COMPLETION_WHISPER_RU, "WHISPER", nil, name)
      MGM_PopQueueAfterSuccessfulSend()
    end
  elseif event == "MAIL_FAILED" then
    mgmSendContext = nil
    mgmArmedPayout = nil
  end
end)

hooksecurefunc("SendMail", function(recipient, _, _)
  if mgmArmedPayout and recipient == mgmArmedPayout.characterName then
    mgmSendContext = mgmArmedPayout
    mgmArmedPayout = nil
  end
end)

--- Call immediately before the streamer clicks Send on a mail prepared from the MGM queue.
function MGM_ArmMailSendForPayout(payoutId, characterName)
  mgmArmedPayout = { payoutId = payoutId, characterName = characterName }
end

function MGM_ClearArmedMailSend()
  mgmArmedPayout = nil
  mgmSendContext = nil
end

--- Desktop: /run NotifyWinnerWhisper("uuid","Name")
function NotifyWinnerWhisper(payoutId, characterName)
  -- TODO: send §9 winner notification whisper (exact Russian body per docs/SPEC.md)
  DEFAULT_CHAT_FRAME:AddMessage(
    "[MGM] NotifyWinnerWhisper not fully implemented: " .. tostring(payoutId)
  )
end

local mgmQueue = {}

--- Desktop: /run ReceiveGold("uuid:Name:copper;...")
function ReceiveGold(dataString)
  local id, name, copper = string.match(dataString, "^([%w%-]+):([^:;]+):(%d+);")
  if id and name and copper then
    table.insert(mgmQueue, { payoutId = id, characterName = name, copper = tonumber(copper) })
    DEFAULT_CHAT_FRAME:AddMessage(
      "[MGM] Queued payout " .. id .. " for " .. name .. " — use Prepare Mail before Send (docs/SPEC.md §9)."
    )
  end
end

--- Call after filling the send-mail frame from the queue, immediately before the streamer clicks Send.
function MGM_PrepareAndArmCurrentQueueHead()
  local q = mgmQueue[1]
  if not q then
    return
  end
  -- TODO: SendMailNameEditBox, subject, MoneyInputFrame_SetCopper (MAIL_SHOW) per docs/SPEC.md
  MGM_ArmMailSendForPayout(q.payoutId, q.characterName)
end

function MGM_PopQueueAfterSuccessfulSend()
  table.remove(mgmQueue, 1)
end
