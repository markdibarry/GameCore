using System;
using System.Collections.Generic;
using GameCore.Utility;

namespace GameCore.GUI;

public static class DialogInterpreter
{
    private static DialogScript s_script = null!;
    private static IStorageContext s_storage = null!;
    private static IEnumerator<ushort> s_enum = null!;

    public static VarType GetReturnType(
        DialogScript dialogScript,
        IStorageContext textStorage,
        IEnumerator<ushort> enumerator)
    {
        return (OpCode)enumerator.Current switch
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
            OpCode.Func => GetFuncReturnType(enumerator),
            OpCode.Var => GetVarType(enumerator),
            OpCode.Assign or
            OpCode.MultAssign or
            OpCode.DivAssign or
            OpCode.AddAssign or
            OpCode.SubAssign => VarType.Void,
            _ => default
        };

        VarType GetVarType(IEnumerator<ushort> enumerator)
        {
            enumerator.MoveNext();
            string varName = dialogScript.InstStrings[enumerator.Current];

            if (DialogBridgeRegister.Properties.TryGetValue(varName, out VarDef? varDef))
                return varDef.VarType;

            if (!textStorage.TryGetValue(varName, out object? obj))
                return default;

            return obj switch
            {
                string => VarType.String,
                bool => VarType.Bool,
                float => VarType.Float,
                _ => default
            };
        }

        VarType GetFuncReturnType(IEnumerator<ushort> enumerator)
        {
            enumerator.MoveNext();
            string funcName = dialogScript.InstStrings[enumerator.Current];

            if (!DialogBridgeRegister.Methods.TryGetValue(funcName, out FuncDef? funcDef))
                return default;

            return funcDef.ReturnType;
        }
    }

    public static float GetFloatInstResult(
        DialogScript script,
        IStorageContext storage,
        IEnumerator<ushort> enumerator)
    {
        SetPrivateFields(script, storage, enumerator);
        float result = EvalFloatExp(false);
        ResetPrivateFields();
        return result;
    }

    public static bool GetBoolInstResult(
        DialogScript script,
        IStorageContext storage,
        IEnumerator<ushort> enumerator)
    {
        SetPrivateFields(script, storage, enumerator);
        bool result = EvalBoolExp(false);
        ResetPrivateFields();
        return result;
    }

    public static string GetStringInstResult(
        DialogScript script,
        IStorageContext storage,
        IEnumerator<ushort> enumerator)
    {
        SetPrivateFields(script, storage, enumerator);
        string result = EvalStringExp(false);
        ResetPrivateFields();
        return result;
    }

    public static void EvalVoidExp(
        DialogScript script,
        IStorageContext storage,
        IEnumerator<ushort> enumerator)
    {
        SetPrivateFields(script, storage, enumerator);

        switch ((OpCode)s_enum.Current)
        {
            case OpCode.Assign:
                EvalAssign();
                break;
            case OpCode.MultAssign:
            case OpCode.DivAssign:
            case OpCode.AddAssign:
            case OpCode.SubAssign:
                EvalMathAssign((OpCode)s_enum.Current);
                break;
            case OpCode.Func:
                EvalFunc();
                break;
        };

        ResetPrivateFields();

        return;

        void EvalAssign()
        {
            s_enum.MoveNext();
            string varName = s_script.InstStrings[s_enum.Current];

            if (DialogBridgeRegister.Properties.TryGetValue(varName, out VarDef? varDef))
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
            else if (s_storage != null)
            {
                switch (GetReturnType(s_script, s_storage, s_enum))
                {
                    case VarType.Float:
                        s_storage.SetValue(varName, EvalFloatExp());
                        break;
                    case VarType.Bool:
                        s_storage.SetValue(varName, EvalBoolExp());
                        break;
                    case VarType.String:
                        s_storage.SetValue(varName, EvalStringExp());
                        break;
                }
            }
        }

        void EvalMathAssign(OpCode instructionType)
        {
            s_enum.MoveNext();
            string varName = s_script.InstStrings[s_enum.Current];
            if (DialogBridgeRegister.Properties.TryGetValue(varName, out VarDef? varDef))
            {
                float originalValue = ((Func<float>)varDef.Getter).Invoke();
                float result = GetOpResult(instructionType, originalValue);
                ((Action<float>)varDef.Setter).Invoke(result);
            }
            else if (s_storage != null)
            {
                if (!s_storage.TryGetValue(varName, out float originalValue))
                    return;
                s_storage.SetValue(varName, GetOpResult(instructionType, originalValue));
            }

            float GetOpResult(OpCode instructionType, float originalValue)
            {
                return instructionType switch
                {
                    OpCode.AddAssign => originalValue + EvalFloatExp(),
                    OpCode.SubAssign => originalValue - EvalFloatExp(),
                    OpCode.MultAssign => originalValue * EvalFloatExp(),
                    OpCode.DivAssign => originalValue / EvalFloatExp(),
                    _ => default
                };
            }
        }
    }

    public static SpeakerUpdate GetSpeakerUpdate(
        DialogScript script,
        IEnumerator<ushort> enumerator,
        IStorageContext storage)
    {
        enumerator.MoveNext();
        string speakerId = script.SpeakerIds[enumerator.Current];
        string? displayName = null, portraitId = null, mood = null;

        while (enumerator.MoveNext())
        {
            switch ((OpCode)enumerator.Current)
            {
                case OpCode.SpeakerSetMood:
                    mood = GetUpdateValue();
                    break;
                case OpCode.SpeakerSetName:
                    displayName = GetUpdateValue();
                    break;
                case OpCode.SpeakerSetPortrait:
                    portraitId = GetUpdateValue();
                    break;
            }
        }

        return new SpeakerUpdate(speakerId, displayName, portraitId, mood);

        string GetUpdateValue()
        {
            enumerator.MoveNext();
            ushort[] updateInst = script.Instructions[enumerator.Current];
            var updateEnum = updateInst.GetEnumerator<ushort>();
            return GetStringInstResult(script, storage, updateEnum);
        }
    }

    private static void SetPrivateFields(DialogScript script, IStorageContext storage, IEnumerator<ushort> enumerator)
    {
        s_script = script;
        s_storage = storage;
        s_enum = enumerator;
    }

    private static void ResetPrivateFields()
    {
        s_script = null!;
        s_storage = null!;
        s_enum = null!;
    }

    private static bool EvalBoolExp(bool moveNext = true)
    {
        if (moveNext)
            s_enum.MoveNext();

        return (OpCode)s_enum.Current switch
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

        bool EvalBool()
        {
            s_enum.MoveNext();
            return s_enum.Current == 1;
        }

        bool EvalEquals()
        {
            s_enum.MoveNext();
            return GetReturnType(s_script, s_storage, s_enum) switch
            {
                VarType.Float => EvalFloatExp() == EvalFloatExp(),
                VarType.Bool => EvalBoolExp() == EvalBoolExp(),
                VarType.String => EvalStringExp() == EvalStringExp(),
                _ => default
            };
        }

        bool EvalLess() => EvalFloatExp() < EvalFloatExp();

        bool EvalGreater() => EvalFloatExp() > EvalFloatExp();

        bool EvalLessEquals() => EvalFloatExp() <= EvalFloatExp();

        bool EvalGreaterEquals() => EvalFloatExp() >= EvalFloatExp();

        bool EvalAnd() => EvalBoolExp() && EvalBoolExp();

        bool EvalOr() => EvalBoolExp() || EvalBoolExp();

        bool EvalNot() => !EvalBoolExp();
    }

    private static string EvalStringExp(bool moveNext = true)
    {
        if (moveNext)
            s_enum.MoveNext();

        return (OpCode)s_enum.Current switch
        {
            OpCode.String => EvalString(),
            OpCode.Var => EvalVar<string>() ?? string.Empty,
            OpCode.Func => (string?)EvalFunc() ?? string.Empty,
            _ => string.Empty
        };

        string EvalString()
        {
            s_enum.MoveNext();
            return s_script.InstStrings[s_enum.Current];
        }
    }

    private static float EvalFloatExp(bool moveNext = true)
    {
        if (moveNext)
            s_enum.MoveNext();

        return (OpCode)s_enum.Current switch
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

        float EvalFloat()
        {
            s_enum.MoveNext();
            return s_script.InstFloats[s_enum.Current];
        }

        float EvalMult() => EvalFloatExp() * EvalFloatExp();

        float EvalDiv() => EvalFloatExp() / EvalFloatExp();

        float EvalAdd() => EvalFloatExp() + EvalFloatExp();

        float EvalSub() => EvalFloatExp() - EvalFloatExp();
    }

    private static T? EvalVar<T>()
    {
        s_enum.MoveNext();
        string varName = s_script.InstStrings[s_enum.Current];

        if (DialogBridgeRegister.Properties.TryGetValue(varName, out VarDef? varDef))
            return ((Func<T>)varDef.Getter).Invoke();

        if (!s_storage.TryGetValue(varName, out object? obj) || obj is not T)
            return default;

        return (T)obj;
    }

    private static object? EvalFunc()
    {
        s_enum.MoveNext();
        string funcName = s_script.InstStrings[s_enum.Current];

        if (!DialogBridgeRegister.Methods.TryGetValue(funcName, out FuncDef? funcDef))
            return default;

        s_enum.MoveNext();
        int argNum = s_enum.Current;

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
