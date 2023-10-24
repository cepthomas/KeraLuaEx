-- Spec for generating ApiLib interop. TODO0 fix this

local M = {}

-- Syntax-specific options.
M.config =
{
    lua_lib_name = "api_lib",
    host_lib_name = "Apiib",
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
        lua_func_name = "my_lua_func", 
        host_func_name = "MyLuaFunc", 
        description = "booga", --OPT
        args =--OPT
        {
            {
                name = "arg_one", 
                type = "S", 
                description = "some strings" --OPT
            },
            {
                name = "arg_two", 
                type = "I", 
                description = "a nice integer" --OPT
            },
            {
                name = "arg_three",  
                type = "T", 
                description = "3 ddddddddd" --OPT
            },
        },
        ret =
        {
            type = "T", 
            description = "a returned thing" --OPT
        }
    },


    {
        description = "Host calls lua",
        lua_func_name = "host_call_lua",
        host_func_name = "HostCallLua",
        args =
        {
            { name = "arg_one", type = "S", description = "some strings" },
            { name = "arg_two", type = "I", description = "a nice integer" },
        },
        ret = { type = "T", description = "a returned thing" }
    },
}

-- Lua calls host.
M.host_export_funcs = 
{

        lua_func_name = "my_lua_func",
        host_func_name = "MyLuaFunc",
        description = "fooga", --OPT
        args = --OPT
        {
            {
                name = "arg_one",
                type = "N",
                description = "kakakakaka" --OPT
            },
        },
        ret =
        {
            type = "B",
            description = "a returned thing" --OPT
        }



    
    {
        description = "Print something for the user",
        lua_func_name = "printex",
        host_func_name = "PrintEx",
        work_func = "PrintExWork", -- Signature is args and ret below.
        args =
        {
            { name = "msg", type = "S", description = "What to tell" },
        },
        -- ret = { type = "B", description = "Status" }
    },
    {
        description = "Get current timer value",
        lua_func_name = "timer",
        host_func_name = "Timer",
        work_func = "TimerWork",
        args =
        {
            { name = "on", type = "B", description = "On or off" },
        },
        ret = { type = "N", description = "Number of msec." }
    },
}

return M