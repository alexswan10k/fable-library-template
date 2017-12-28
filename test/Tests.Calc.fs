module Tests.Calc

open QUnit

registerModule "Calc Tests"

testCase "Add works" <| fun test ->
    let result = Calc.add 3 5 
    test.areEqual result 8

testCase "Failing test" <| fun test ->
    match [1 .. 3] with
    | [1;2] -> test.pass()
    | otherwise -> test.unexpected otherwise

testCaseAsync "Async tests too" <| fun test ->
    async {
        do! Async.Sleep 1000
        test.pass()
    }