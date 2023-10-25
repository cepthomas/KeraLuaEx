

local M = { }

function M.method3(param)
    print("method3()")
    local y = 10 + param;
    return y * 2
end

return M