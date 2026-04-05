--[[
  Unit tests: MimironsGoldOMatic.Core (parsing, names, WHO JSON line).
]]

local lu = require("luaunit")

dofile(CORE_LUA)

TestCore = {}

function TestCore:test_trim()
  lu.assertEquals(MimironsGoldOMaticCore.Trim("  abc  "), "abc")
  lu.assertEquals(MimironsGoldOMaticCore.Trim(nil), "")
end

function TestCore:test_short_name_realm()
  lu.assertEquals(MimironsGoldOMaticCore.ShortName("Player-Server"), "Player")
  lu.assertEquals(MimironsGoldOMaticCore.ShortName("Solo"), "Solo")
end

function TestCore:test_names_equal_case_and_realm()
  lu.assertTrue(MimironsGoldOMaticCore.NamesEqual("AbC", "abc"))
  lu.assertTrue(MimironsGoldOMaticCore.NamesEqual("Player-Realm", "player"))
  lu.assertFalse(MimironsGoldOMaticCore.NamesEqual("A", "B"))
end

function TestCore:test_json_escape()
  lu.assertEquals(MimironsGoldOMaticCore.JsonEscape([[a\b"c]]), [[a\\b\"c]])
end

function TestCore:test_build_who_log_line_deterministic()
  local line = MimironsGoldOMaticCore.BuildWhoLogLine("sc-1", 'N"x', true, "2020-05-01T12:00:00Z")
  lu.assertStrContains(line, "[MGM_WHO]")
  lu.assertStrContains(line, [["spinCycleId":"sc-1"]])
  lu.assertStrContains(line, [["characterName":"N\\\"x"]])
  lu.assertStrContains(line, [["online":true]])
  lu.assertStrContains(line, [["capturedAt":"2020-05-01T12:00:00Z"]])
end

function TestCore:test_find_matching_who_name()
  local function n()
    return 2
  end
  local function info(i)
    if i == 1 then
      return "Other"
    end
    if i == 2 then
      return "Target-Realm"
    end
    return nil
  end
  lu.assertTrue(MimironsGoldOMaticCore.FindMatchingWhoName("target", n, info))
  lu.assertFalse(MimironsGoldOMaticCore.FindMatchingWhoName("nobody", n, info))
end

function TestCore:test_parse_gold_segment()
  local row = MimironsGoldOMaticCore.ParseGoldSegment("payout-1:CharName:5000")
  lu.assertNotNil(row)
  lu.assertEquals(row.payoutId, "payout-1")
  lu.assertEquals(row.characterName, "CharName")
  lu.assertEquals(row.copper, 5000)
  lu.assertEquals(row.state, "READY")
  lu.assertNil(MimironsGoldOMaticCore.ParseGoldSegment("bad"))
  lu.assertNil(MimironsGoldOMaticCore.ParseGoldSegment(""))
end

function TestCore:test_parse_gold_segment_trim()
  local row = MimironsGoldOMaticCore.ParseGoldSegment("  id:name:1  ")
  lu.assertEquals(row.copper, 1)
end
