
local api = require "api_lib" -- C# module

-- Create the namespace/module.
local M = {}

-- Functions implemented in C#.
api.printex("Loading luaex.lua!")
api.timer(true)


M.xxxxxxxx = "tracer"

-- Local vars.
local tune_string = "tune" 
local const_string <const> = "trig" 
local index = 1

-- Global vars.
g_string = "booga booga"
g_number = 7.654
g_int = 80808
g_list_number = { 2.2, 56.3, 98.77, 2.303 }

-- Global func.
function g_func(i)
  api.printex("g_func " .. i)
  return i * 4
end

-- Module vars.
M.m_bool = false
M.m_table = { dev_type="bing_bong", channel=10, abool=true }
M.m_list_int = { 2, 56, 98, 2 }
M.m_list_string = { "a", "string", "with" }


-- Table of tables.
M.things =
{
  tune = { dev_type="midi_in", channel=1, long_list={ 44, 77, 101 } },
  trig = { dev_type="virt_key", channel=2, adouble=1.234 },
  whiz = { dev_type="bing_bong", double_table={ 145.89, 71.23, 909.555 }, channel=99 },
  pers = { dev_type="abra", string_table={ 1, 23, 88, 22 }, channel=111, abool=false },
  --invalid_table = { atable={ 1, 2, 3, "ppp", 88.22 }, channel=10, abool=true }
}

-- Mixed type array.
M.invalid_table = { 1, 2, 3, "ppp", 88.22 }


-- Functions called from C#.
function M.funcy(s)
  index = index + 1
  api.printex("funcy " .. #s .. " " .. index)
  return #s + 3
end

function M.calc(addends, suffix)
  sum = 0
  for k, v in pairs(addends) do
    api.printex(k .. ":" .. v)
    sum = sum + v
  end
  return { str=string.format('>>>%d_%s<<<', sum, suffix), sum=sum }
end

-- How long is it?
local msec = api.timer(false)
api.printex("this took " .. msec .. " msec")


----------------------------------------------------------------------------
-- Module initialization.

-- Seed the randomizer.
local seed = os.time()
math.randomseed(seed)
M.seed = seed

-- Return the module.
return M
