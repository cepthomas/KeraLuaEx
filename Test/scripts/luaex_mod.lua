
local api = require "api_lib" -- C# module

-- Create the namespace/module.
local M = {}

-- Functions implemented in C#.
api.printex("Loading luaex_mod.lua!")
api.timer(true)

-- Local vars.
local yikes = "xxxxxxx"

-- Global vars.
g_int = 71717


-- Module vars.
M.m_string = "Here I am"
M.m_bool = false
M.m_table = { dev_type="bing_bong", channel=10, abool=true }
M.m_list_int = { 2, 56, 98, 2 }


-- Functions called from C#.
function M.funcmod(s)
  api.printex("funcmod " .. #s)
  return #s + 3
end

function M.calcmod(addends, suffix)
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
