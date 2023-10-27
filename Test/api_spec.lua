-- Spec for generating ApiLib interop.
local M = {}

-- Syntax-specific options.
M.config =
{
    lua_lib_name = "api_lib",
    host_lib_name = "ApiLib",
    namespace = "KeraLuaEx.Test",
    add_refs =
    {
        "System.Diagnostics",
    },
}

-- Host calls lua.
M.lua_export_funcs =
{
    {
        lua_func_name = "do_operation",
        host_func_name = "DoOperation",
        description = "Host asks script to do something",
        args =--OPT
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
M.host_export_funcs =
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
        args = --OPT
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
