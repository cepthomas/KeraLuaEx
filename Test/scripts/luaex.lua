
-- Function implemented in C#.
printex("Loading luaex.lua!")

math.randomseed(os.time())

-- C# timer.
timer(true)

-- Locals.
local tune_string = "tune" 
local const_string <const> = "trig" 
local index = 1

-- Misc globals.
g_string = "booga booga"
g_number = 7.654
g_int = 80808
g_bool = false
g_table = { dev_type="bing_bong", channel=10, abool=true }
g_list_int = { 2, 56, 98, 2 }
g_list_number = { 2.2, 56.3, 98.77, 2.303 }
g_list_string = { "a", "string", "with" }


-- Table of tables.
things =
{
  tune = { dev_type="midi_in", channel=1, long_list={ 44, 77, 101 } },
  trig = { dev_type="virt_key", channel=2, adouble=1.234 },
  whiz = { dev_type="bing_bong", double_table={ 145.89, 71.23, 909.555 }, channel=99 },
  per = { dev_type="abra", string_table={ 1, 23, 88, 22 }, channel=111, abool=false },
  --invalid_table = { atable={ 1, 2, 3, "ppp", 88.22 }, channel=10, abool=true }
}

-- Mixed type array.
invalid_table = { 1, 2, 3, "ppp", 88.22 }


-- Functions called from C#.
function g_func(s)
  index = index + 1
  printex("g_func " .. #s .. " " .. index)
  return #s + 3
end

function calc(addends, suffix)
  sum = 0
  for k, v in pairs(addends) do
    printex(k .. ":" .. v)
    sum = sum + v
  end
  return { str=string.format('>>>%d_%s<<<', sum, suffix), sum=sum }
end

-- How long?
msec = timer(false)
printex("this took " .. msec .. " msec")



-- --------------------------------------------

---- sequences ----
-- when, named chords, volume, duration(def=0.1)
seq_KeysChorus = [[
0.0     F4      0.7     0.2
2.0     F5      0.7      0.2
5.4     B3.m7   KEYS_VOL     0.2
-- etc...
7.4     B2.m7   0.7     0.2
8.0 end?]]


-- This is probably the better way to do drums and rhythmic instruments. But could be notes too.
-- |........|........|........|........|........|........|........|........|
seq_DrumsVerse = [[
|8       |        |8       |        |8       |        |8       |        | AcousticBassDrum  DRUM_VOL
|    8   |        |    8   |    8   |    8   |        |    8   |    8   | AcousticSnare     DRUM_VOL*0.9
|        |     8 8|        |     8 8|        |     8 8|        |     8 8| ClosedHiHat       DRUM_VOL*1.1
]]

---- sections ----
sect_Middle = [[
keys    seq_KeysChorus  seq_KeysChorus  seq_KeysChorus  seq_KeysChorus
drums   seq_DrumsChorus seq_DrumsChorus seq_DrumsChorus seq_DrumsChorus
bass    seq_BassChorus  seq_BassChorus  seq_BassChorus  seq_BassChorus
synth   algo_func       nil             algo_func       seq_Dynamic
end
]]
