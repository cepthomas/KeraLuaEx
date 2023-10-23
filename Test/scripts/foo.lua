
local m = require("module1")

function method1()
    local x = 10;
    print("method1()")
    m.method3(x)
end

function method2()
    local x = 10;
    print("method2()")
    method1()
end

