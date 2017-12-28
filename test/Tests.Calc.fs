module Tests.Calc

open QUnit

registerModule "Calc Tests"

testCase "Simple test" <| fun test ->
    let result = Calc.add 3 5 
    test.areEqual 8 result 

testCase "Square works" <| fun test ->
    let result = Calc.square 5
    test.areEqual 25 result 

testCase "Cube works" <| fun test ->
    test.areEqual 125 (Calc.cube 5) 

testCase "factorial works" <| fun test ->
    test.areEqual 120 (Calc.factorial 5)

testCaseAsync "Async test" <| fun test ->
    async {
        do! Async.Sleep 1000
        test.pass()
    }