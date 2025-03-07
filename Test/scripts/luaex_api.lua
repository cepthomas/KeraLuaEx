
local api = require("luainterop") -- C# module

-- Functions called by lua implemented in C#.
v = api.printex("Loading luaex.lua!")
y = api.timer(true)

-- Functions called from C#.
function do_operation(arg_one, arg_two)
	api.printex("do_operation(): "..arg_one.." "..arg_two)
	local ret = { sret = arg_one:reverse(), iret = arg_two / 2 }
	return ret
end

-- How long is it?
local msec = api.timer(false)
api.printex("this took "..msec.." msec")
