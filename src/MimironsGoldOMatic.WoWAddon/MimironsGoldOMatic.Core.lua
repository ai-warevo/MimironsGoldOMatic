--[[
  Pure helpers shared by the addon (WoW 3.3.5a) and headless Lua tests.
  Normative: docs/SPEC.md §8 (MGM_WHO JSON shape).
]]

MimironsGoldOMaticCore = MimironsGoldOMaticCore or {}

local Core = MimironsGoldOMaticCore

function Core.Trim(s)
  if not s then
    return ""
  end
  return (string.gsub(s, "^%s*(.-)%s*$", "%1"))
end

function Core.ShortName(name)
  if not name then
    return ""
  end
  local base = string.match(name, "^([^%-]+)")
  return base or name
end

function Core.NamesEqual(a, b)
  return string.lower(Core.ShortName(a)) == string.lower(Core.ShortName(b))
end

function Core.UtcIso8601()
  local t
  if type(_G.date) == "function" then
    t = _G.date("!*t")
  elseif os and os.date then
    t = os.date("!*t")
  else
    return "1970-01-01T00:00:00Z"
  end
  if not t then
    if os and os.date then
      return os.date("!%Y-%m-%dT%H:%M:%SZ")
    end
    return "1970-01-01T00:00:00Z"
  end
  return string.format(
    "%04d-%02d-%02dT%02d:%02d:%02dZ",
    t.year,
    t.month,
    t.day,
    t.hour,
    t.min,
    t.sec
  )
end

function Core.JsonEscape(s)
  if not s then
    return ""
  end
  s = string.gsub(s, "\\", "\\\\")
  s = string.gsub(s, '"', '\\"')
  return s
end

--- Full chat line: [MGM_WHO] + compact JSON (SPEC §8). Optional capturedAt for tests.
function Core.BuildWhoLogLine(spinCycleId, characterName, online, capturedAtOverride)
  local capturedAt = capturedAtOverride or Core.UtcIso8601()
  local json = string.format(
    '{"schemaVersion":1,"spinCycleId":"%s","characterName":"%s","online":%s,"capturedAt":"%s"}',
    Core.JsonEscape(spinCycleId),
    Core.JsonEscape(characterName),
    online and "true" or "false",
    capturedAt
  )
  return "[MGM_WHO]" .. json
end

function Core.FindMatchingWhoName(expectedName, getNumResults, getNameAtIndex)
  local n = (getNumResults and getNumResults()) or 0
  if n == 0 then
    return false
  end
  for i = 1, n do
    local name = getNameAtIndex(i)
    if name and Core.NamesEqual(name, expectedName) then
      return true
    end
  end
  return false
end

--- Parses one semicolon-separated ReceiveGold segment (ReceiveGold global).
function Core.ParseGoldSegment(piece)
  piece = Core.Trim(piece)
  if piece == "" then
    return nil
  end
  local id, name, copperStr = string.match(piece, "^([%w%-]+):([^:]+):(%d+)$")
  if not id or not name or not copperStr then
    return nil
  end
  return {
    payoutId = id,
    characterName = name,
    copper = tonumber(copperStr) or 0,
    state = "READY",
  }
end
