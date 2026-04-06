--[[
  Integration tests: load addon with wow_api_mock; exercise globals and OnEvent.
]]

local lu = require("luaunit")

local function loadFreshAddon()
  WOW_MOCK_CREATED_FRAMES = {}
  WOW_MOCK_SENDMAIL_HOOKS = {}
  SendChatMessage_calls = {}
  MailFrame = nil
  SendMailNameEditBox = nil
  SendMailSubjectEditBox = nil
  SendMailMoney = nil

  dofile(MOCK_LUA)
  DEFAULT_CHAT_FRAME.messages = {}
  GetNumWhoResults = function()
    return 0
  end
  GetWhoInfo = function()
    return nil
  end
  dofile(CORE_LUA)
  dofile(ADDON_LUA)
end

local function eventFrame()
  return WOW_MOCK_CREATED_FRAMES[1]
end

local function fireEvent(ev, ...)
  local f = eventFrame()
  local fn = f:GetScript("OnEvent")
  lu.assertNotNil(fn)
  fn(f, ev, ...)
end

TestAddonIntegration = {}

function TestAddonIntegration:test_notify_winner_and_whisper_accept()
  loadFreshAddon()
  NotifyWinnerWhisper("uuid-123", "Winner")
  lu.assertEquals(#SendChatMessage_calls, 1)
  lu.assertStrContains(SendChatMessage_calls[1].msg, "!twgold")
  fireEvent("CHAT_MSG_WHISPER", "!twgold", "Winner")
  lu.assertEquals(#DEFAULT_CHAT_FRAME.messages, 1)
  lu.assertEquals(DEFAULT_CHAT_FRAME.messages[1], "[MGM_ACCEPT:uuid-123]")
end

function TestAddonIntegration:test_whisper_wrong_body_ignored()
  loadFreshAddon()
  NotifyWinnerWhisper("u1", "Winner")
  fireEvent("CHAT_MSG_WHISPER", "not-twgold", "Winner")
  lu.assertEquals(#DEFAULT_CHAT_FRAME.messages, 0)
end

function TestAddonIntegration:test_who_list_emits_mgm_who()
  loadFreshAddon()
  GetNumWhoResults = function()
    return 1
  end
  GetWhoInfo = function(i)
    if i == 1 then
      return "Hero"
    end
    return nil
  end
  MGM_RunWhoForSpin("cycle-9", "Hero")
  fireEvent("WHO_LIST_UPDATE")
  lu.assertTrue(#DEFAULT_CHAT_FRAME.messages >= 1)
  local last = DEFAULT_CHAT_FRAME.messages[#DEFAULT_CHAT_FRAME.messages]
  lu.assertStrContains(last, "[MGM_WHO]")
  lu.assertStrContains(last, "cycle-9")
  lu.assertStrContains(last, [["online":true]])
end

function TestAddonIntegration:test_show_queue_panel_after_receive_gold()
  loadFreshAddon()
  ReceiveGold("a:One:10;b:Two:20")
  MGM_ShowQueuePanel()
  local found = false
  for _, fr in ipairs(WOW_MOCK_CREATED_FRAMES) do
    if fr.name == "MGMQueuePanel" then
      found = true
      break
    end
  end
  lu.assertTrue(found)
end

function TestAddonIntegration:test_slash_update_emits_update_tag_and_messages()
  loadFreshAddon()
  SlashCmdList["MGM"]("update")
  lu.assertEquals(#DEFAULT_CHAT_FRAME.messages, 3)
  lu.assertStrContains(DEFAULT_CHAT_FRAME.messages[1], "проверка обновлений")
  lu.assertStrContains(DEFAULT_CHAT_FRAME.messages[2], "Desktop")
  lu.assertEquals(DEFAULT_CHAT_FRAME.messages[3], "[MGM_UPDATE_CHECK]")
end
