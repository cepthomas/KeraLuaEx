
local api = require("api_lib") -- C# module

-- Functions called by lua implemented in C#.
api.printex("Loading luaex.lua!")
api.timer(true)

-- Functions called from C#.
function host_call_lua(arg_one, arg_two)
    api.printex("host_call_lua(): " .. arg_one .. " " .. arg_two)
    ret = { sret = arg_one:reverse(), iret = arg_two / 2 }
    return ret
end

-- How long is it?
local msec = api.timer(false)
api.printex("this took " .. msec .. " msec")
