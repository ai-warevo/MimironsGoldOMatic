--[[
  Minimal WoW 3.3.5a API stubs so MimironsGoldOMatic.lua can load outside the client.
]]

WOW_MOCK_CREATED_FRAMES = {}
WOW_MOCK_SENDMAIL_HOOKS = {}

local function pushFrame(f)
  table.insert(WOW_MOCK_CREATED_FRAMES, f)
  return f
end

local function makeFontString()
  return {
    text = "",
    SetPoint = function() end,
    SetText = function(self, t)
      self.text = t or ""
    end,
    SetWidth = function() end,
    SetJustifyH = function() end,
  }
end

--- UIParent / Minimap are not tracked — integration tests use WOW_MOCK_CREATED_FRAMES[1] = addon event frame.
local function buildFrame(kind, name, parent, template)
  return {
    kind = kind or "Frame",
    name = name,
    parent = parent,
    template = template,
    scripts = {},
    events = {},
    shown = false,
    alpha = 1,
    RegisterEvent = function(self, e)
      table.insert(self.events, e)
    end,
    UnregisterAllEvents = function() end,
    SetScript = function(self, k, fn)
      self.scripts[k] = fn
    end,
    GetScript = function(self, k)
      return self.scripts[k]
    end,
    Show = function(self)
      self.shown = true
    end,
    Hide = function(self)
      self.shown = false
    end,
    IsShown = function(self)
      return self.shown
    end,
    IsVisible = function(self)
      return self.shown
    end,
    SetWidth = function() end,
    SetHeight = function() end,
    SetPoint = function() end,
    ClearAllPoints = function() end,
    SetBackdrop = function() end,
    SetBackdropColor = function() end,
    SetBackdropBorderColor = function() end,
    SetMovable = function() end,
    EnableMouse = function() end,
    RegisterForDrag = function() end,
    RegisterForClicks = function() end,
    SetFrameStrata = function() end,
    SetNormalTexture = function() end,
    SetPushedTexture = function() end,
    SetHighlightTexture = function() end,
    SetAlpha = function(self, a)
      self.alpha = a
    end,
    GetAlpha = function(self)
      return self.alpha or 1
    end,
    StartMoving = function() end,
    StopMovingOrSizing = function() end,
    CreateFontString = function()
      return makeFontString()
    end,
    CreateFrame = function(self, k, n, p, t)
      return CreateFrame(k, n, p or self, t)
    end,
    SetEnabled = function() end,
    SetText = function(self, t)
      self._text = t
    end,
  }
end

function CreateFrame(kind, name, parent, template)
  return pushFrame(buildFrame(kind, name, parent, template))
end

--- Populated by addons at load time (e.g. SlashCmdList["MGM"] = fn); must exist before dofile(addon).
SlashCmdList = {}

DEFAULT_CHAT_FRAME = {
  messages = {},
  AddMessage = function(self, msg)
    table.insert(self.messages, msg)
  end,
}

SendChatMessage_calls = {}

function SendChatMessage(msg, chatType, _, target)
  table.insert(SendChatMessage_calls, { msg = msg, chatType = chatType, target = target })
end

function hooksecurefunc(name, fn)
  if name == "SendMail" then
    WOW_MOCK_SENDMAIL_HOOKS = { fn }
  end
end

function WOW_MOCK_FireSendMailHook(recipient, subject, body)
  for _, h in ipairs(WOW_MOCK_SENDMAIL_HOOKS) do
    h(recipient, subject, body)
  end
end

UIParent = buildFrame("Frame", "UIParent")
Minimap = buildFrame("Frame", "Minimap")

GameTooltip = {
  SetOwner = function() end,
  SetText = function() end,
  AddLine = function() end,
  Show = function() end,
  Hide = function() end,
}

function UnitAffectingCombat()
  return false
end

MailFrame = nil
SendMailNameEditBox = nil
SendMailSubjectEditBox = nil
SendMailMoney = nil

function MoneyInputFrame_SetCopper() end

GetNumWhoResults = function()
  return 0
end

GetWhoInfo = function()
  return nil
end

function RunMacroText() end
