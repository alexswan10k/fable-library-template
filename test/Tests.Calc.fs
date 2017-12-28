module Tests.Calc

open QUnit

registerModule "Calc Tests"

testCase "Simple test" <| fun test ->
    let result = Calc.add 3 5 
    test.areEqual result 8


testCaseAsync "Async test" <| fun test ->
    async {
        do! Async.Sleep 1000
        test.pass()
    }