namespace GameCore.GUI;

public enum OpCode : ushort
{
    Undefined,
    Float,
    String,
    Bool,
    // name string
    Var,
    // name string, number of arguments (float), expressions...
    Func,
    // float, float
    Mult,
    Div,
    Add,
    Sub,
    LessEquals,
    GreaterEquals,
    Less,
    Greater,
    // expression, expression
    Equals,
    NotEquals,
    // bool, bool
    And,
    Or,
    Not,
    // Variable index, expression
    Assign,
    MultAssign,
    DivAssign,
    AddAssign,
    SubAssign,
    // toggle (bool)
    Auto,
    // Section Index
    Goto,
    NewLine,
    // multiplier (float)
    Speed,
    SpeakerSet,
    // SpeakerId (float), Name (expression)
    SpeakerSetName,
    // SpeakerId (float), Mood (expression)
    SpeakerSetMood,
    // SpeakerId (float), Portrait (expression)
    SpeakerSetPortrait,
    Choice
}
