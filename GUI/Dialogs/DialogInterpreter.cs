using System;
using System.Collections.Generic;

namespace GameCore.GUI;

public class DialogInterpreter
{
    public DialogInterpreter(DialogBridgeRegister register, DialogScript dialogScript, TextStorage textStorage)
    {
        _register = register;
        _dialogScript = dialogScript;
        _textStorage = textStorage;
        _instructions = Array.Empty<ushort>();
        _index = -1;
    }

    private readonly DialogBridgeRegister _register;
    private readonly DialogScript _dialogScript;
    private readonly TextStorage _textStorage;
    private ushort[] _instructions;
    private int _index;

    public VarType GetReturnType(ushort[] instructions, int index)
    {
        return (OpCode)instructions[index] switch
        {
            OpCode.String => VarType.String,
            OpCode.Float or
            OpCode.Mult or
            OpCode.Div or
            OpCode.Add or
            OpCode.Sub => VarType.Float,
            OpCode.Bool or
            OpCode.Less or
            OpCode.Greater or
            OpCode.LessEquals or
            OpCode.GreaterEquals or
            OpCode.Equals or
            OpCode.NotEquals or
            OpCode.Not => VarType.Bool,
            OpCode.Func => GetFuncReturnType(),
            OpCode.Var => GetVarType(),
            OpCode.Assign or
            OpCode.MultAssign or
            OpCode.DivAssign or
            OpCode.AddAssign or
            OpCode.SubAssign => VarType.Void,
            _ => default
        };

        VarType GetVarType()
        {
            string varName = _dialogScript.InstStrings[instructions[index + 1]];
            if (_register.Properties.TryGetValue(varName, out VarDef? varDef))
                return varDef.VarType;
            if (_textStorage == null || !_textStorage.TryGetValue(varName, out object? obj))
                return default;
            return obj switch
            {
                string => VarType.String,
                bool => VarType.Bool,
                float => VarType.Float,
                _ => default
            };
        }

        VarType GetFuncReturnType()
        {
            string funcName = _dialogScript.InstStrings[instructions[index + 1]];
            if (!_register.Methods.TryGetValue(funcName, out FuncDef? funcDef))
                return default;
            return funcDef.ReturnType;
        }
    }

    public float GetFloatInstResult(ushort[] instructions)
    {
        _instructions = instructions;
        float result = EvalFloatExp();
        _instructions = Array.Empty<ushort>();
        _index = -1;
        return result;
    }

    public bool GetBoolInstResult(ushort[] instructions)
    {
        _instructions = instructions;
        bool result = EvalBoolExp();
        _instructions = Array.Empty<ushort>();
        _index = -1;
        return result;
    }

    public string GetStringInstResult(ushort[] instructions)
    {
        _instructions = instructions;
        string result = EvalStringExp();
        _instructions = Array.Empty<ushort>();
        _index = -1;
        return result;
    }

    public void EvalVoidInst(ushort[] instructions)
    {
        _instructions = instructions;
        EvalVoidExp();
        _instructions = Array.Empty<ushort>();
        _index = -1;
    }

    private bool EvalBoolExp()
    {
        return (OpCode)_instructions[++_index] switch
        {
            OpCode.Bool => EvalBool(),
            OpCode.Less => EvalLess(),
            OpCode.Greater => EvalGreater(),
            OpCode.LessEquals => EvalLessEquals(),
            OpCode.GreaterEquals => EvalGreaterEquals(),
            OpCode.Equals => EvalEquals(),
            OpCode.NotEquals => !EvalEquals(),
            OpCode.Not => EvalNot(),
            OpCode.And => EvalAnd(),
            OpCode.Or => EvalOr(),
            OpCode.Var => EvalVar<bool>(),
            OpCode.Func => (bool?)EvalFunc() ?? default,
            _ => default
        };
    }

    private string EvalStringExp()
    {
        return (OpCode)_instructions[++_index] switch
        {
            OpCode.String => EvalString(),
            OpCode.Var => EvalVar<string>() ?? string.Empty,
            OpCode.Func => (string?)EvalFunc() ?? string.Empty,
            _ => string.Empty
        };
    }

    private float EvalFloatExp()
    {
        return (OpCode)_instructions[++_index] switch
        {
            OpCode.Float => EvalFloat(),
            OpCode.Mult => EvalMult(),
            OpCode.Div => EvalDiv(),
            OpCode.Add => EvalAdd(),
            OpCode.Sub => EvalSub(),
            OpCode.Var => EvalVar<float>(),
            OpCode.Func => (float?)EvalFunc() ?? default,
            _ => default
        };
    }

    private void EvalVoidExp()
    {
        switch ((OpCode)_instructions[++_index])
        {
            case OpCode.Assign:
                EvalAssign();
                break;
            case OpCode.MultAssign:
            case OpCode.DivAssign:
            case OpCode.AddAssign:
            case OpCode.SubAssign:
                EvalMathAssign((OpCode)_instructions[_index]);
                break;
            case OpCode.Func:
                EvalFunc();
                break;
        };
        return;
    }

    private bool EvalEquals()
    {
        return GetReturnType(_instructions, _index + 1) switch
        {
            VarType.Float => EvalFloatExp() == EvalFloatExp(),
            VarType.Bool => EvalBoolExp() == EvalBoolExp(),
            VarType.String => EvalStringExp() == EvalStringExp(),
            _ => default
        };
    }

    private bool EvalLess() => EvalFloatExp() < EvalFloatExp();

    private bool EvalGreater() => EvalFloatExp() > EvalFloatExp();

    private bool EvalLessEquals() => EvalFloatExp() <= EvalFloatExp();

    private bool EvalGreaterEquals() => EvalFloatExp() >= EvalFloatExp();

    private bool EvalAnd() => EvalBoolExp() && EvalBoolExp();

    private bool EvalOr() => EvalBoolExp() || EvalBoolExp();

    private bool EvalNot() => !EvalBoolExp();

    private float EvalMult() => EvalFloatExp() * EvalFloatExp();

    private float EvalDiv() => EvalFloatExp() / EvalFloatExp();

    private float EvalAdd() => EvalFloatExp() + EvalFloatExp();

    private float EvalSub() => EvalFloatExp() - EvalFloatExp();

    private bool EvalBool() => _instructions[++_index] == 1;

    private float EvalFloat() => _dialogScript.InstFloats[_instructions[++_index]];

    private string EvalString() => _dialogScript.InstStrings[_instructions[++_index]];

    private void EvalAssign()
    {
        string varName = _dialogScript.InstStrings[_instructions[++_index]];
        if (_register.Properties.TryGetValue(varName, out VarDef? varDef))
        {
            switch (varDef.VarType)
            {
                case VarType.Float:
                    ((Action<float>)varDef.Setter).Invoke(EvalFloatExp());
                    break;
                case VarType.Bool:
                    ((Action<bool>)varDef.Setter).Invoke(EvalBoolExp());
                    break;
                case VarType.String:
                    ((Action<string>)varDef.Setter).Invoke(EvalStringExp());
                    break;
            }
        }
        else if (_textStorage != null)
        {
            switch (GetReturnType(_instructions, _index + 1))
            {
                case VarType.Float:
                    _textStorage.SetValue(varName, EvalFloatExp());
                    break;
                case VarType.Bool:
                    _textStorage.SetValue(varName, EvalBoolExp());
                    break;
                case VarType.String:
                    _textStorage.SetValue(varName, EvalStringExp());
                    break;
            }
        }
    }

    private void EvalMathAssign(OpCode instructionType)
    {
        string varName = _dialogScript.InstStrings[_instructions[++_index]];
        if (_register.Properties.TryGetValue(varName, out VarDef? varDef))
        {
            float originalValue = ((Func<float>)varDef.Getter).Invoke();
            float result = instructionType switch
            {
                OpCode.AddAssign => originalValue + EvalFloatExp(),
                OpCode.SubAssign => originalValue - EvalFloatExp(),
                OpCode.MultAssign => originalValue * EvalFloatExp(),
                OpCode.DivAssign => originalValue / EvalFloatExp(),
                _ => default
            };
            ((Action<float>)varDef.Setter).Invoke(result);
        }
        else if (_textStorage != null)
        {
            if (!_textStorage.TryGetValue(varName, out float originalValue))
                return;
            _textStorage.SetValue(varName, instructionType switch
            {
                OpCode.AddAssign => originalValue + EvalFloatExp(),
                OpCode.SubAssign => originalValue - EvalFloatExp(),
                OpCode.MultAssign => originalValue * EvalFloatExp(),
                OpCode.DivAssign => originalValue / EvalFloatExp(),
                _ => default
            });
        }

    }

    private T? EvalVar<T>()
    {
        string varName = _dialogScript.InstStrings[_instructions[++_index]];
        if (_register.Properties.TryGetValue(varName, out VarDef? varDef))
            return ((Func<T>)varDef.Getter).Invoke();
        if (!_textStorage.TryGetValue(varName, out object? obj) || obj is not T)
            return default;
        return (T)obj;
    }

    private object? EvalFunc()
    {
        string funcName = _dialogScript.InstStrings[_instructions[++_index]];
        if (!_register.Methods.TryGetValue(funcName, out FuncDef? funcDef))
            return default;
        int argNum = _instructions[++_index];
        if (argNum == 0)
            return funcDef.Method.Invoke(Array.Empty<object>());
        List<object> args = new();
        for (int i = 0; i < argNum; i++)
        {
            switch (funcDef.ArgTypes[i])
            {
                case VarType.Float:
                    args.Add(EvalFloatExp());
                    break;
                case VarType.Bool:
                    args.Add(EvalBoolExp());
                    break;
                case VarType.String:
                    args.Add(EvalStringExp());
                    break;
            };
        }
        return funcDef.Method.Invoke(args.ToArray());
    }
}
