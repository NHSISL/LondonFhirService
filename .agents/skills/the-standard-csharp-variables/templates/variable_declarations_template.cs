// ---
// skill: the-standard-csharp-variables
// type: template
// source-section: "0. Variables"
// ---

// ✅ Clear type — var
var {entity} = new {EntityType}();

// ✅ Semi-clear type — explicit
{EntityType} {entity} = Get{Entity}();

// ✅ Null/default variable
{EntityType} no{Entity} = null;
int no{Concept}Count = 0;

// ✅ Collection — plural name, var
var {entities} = new List<{EntityType}>();

// ✅ Single-property instantiation — declare then assign
var {inputEntity} = new {EntityEventType}();
{inputEntity}.{Property} = {value};

// ✅ Multi-property instantiation — initializer block
var {entity} = new {EntityType}
{
    {Property1} = {value1},
    {Property2} = {value2}
};

// ✅ Long declaration — break at =
{ExplicitType} {longNamedVariable} =
    await {LongMethodNameAsync}();

// ✅ Stacked single-line declarations
{Type1} {var1} = {expr1};
{Type2} {var2} = {expr2};

// ✅ Multi-line declaration surrounded by blank lines
{Type1} {var1} = {expr1};

{ExplicitType} {longNamedVariable} =
    await {LongMethodNameAsync}();

{Type3} {var3} = {expr3};
