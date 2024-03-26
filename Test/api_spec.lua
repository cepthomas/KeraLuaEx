-- Spec for generating ApiLib interop.
local M = {}

-- Syntax-specific options.
M.config =
{
    lua_lib_name = "api_lib",
    namespace = "KeraLuaEx.Test",
    class = "LuaInterop",
    add_refs = { "System.Diagnostics" }
}

-- Host calls lua.
M.script_funcs =
{
    {
        lua_func_name = "do_operation",
        host_func_name = "DoOperation",
        description = "Host asks script to do something",
        args =
        {
            {
                name = "arg_one",
                type = "S",
                description = "a string"
            },
            {
                name = "arg_two",
                type = "I",
                description = "an integer"
            },
        },
        ret =
        {
            type = "T",
            description = "some calculated values"
        }
    },
}

-- Lua calls host.
M.host_funcs =
{
    {
        lua_func_name = "printex",
        host_func_name = "PrintEx",
        description = "Print something for the user",
        args =
        {
            {
                name = "msg",
                type = "S",
                description = "What to tell"
            },
        },
        ret =
        {
            type = "B",
            description = "Status"
        }
    },
    {
        lua_func_name = "timer",
        host_func_name = "Timer",
        description = "Get current timer value",
        args =
        {
            {
                name = "on",
                type = "B",
                description = "On or off"
            },
        },
        ret =
        {
            type = "N",
            description = "Number of msec"
        }
    },
}

return M
