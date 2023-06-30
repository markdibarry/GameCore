using System;
using System.Collections.Generic;
using GameCore.Utility;

namespace GameCore.GUI;

public static class DialogInterpreter
{
    private static DialogScript s_script = null!;
    private static IStorageContext s_storage = null!;
    private static IndexedArray<ushort> s_indexedArray = s_defaultIndexedArray;
    private static readonly IndexedArray<ushort> s_defaultIndexedArray = new(Array.Empty<ushort>());

    public static VarType GetReturnType(
        DialogScript dialogScript,
        IStorageContext textStorage,
        IndexedArray<ushort> indexedArray)
    {
        return (OpCode)indexedArray.Current switch
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
            OpCode.Func => GetFuncReturnType(indexedArray),
            OpCode.Var => GetVarType(indexedArray),
            OpCode.Assign or
            OpCode.MultAssign or
            OpCode.DivAssign or
            OpCode.AddAssign or
            OpCode.SubAssign => VarType.Void,
            _ => default
        };

        VarType GetVarType(IndexedArray<ushort> indexedArray)
        {
            ushort nameIndex = indexedArray[indexedArray.Index + 1];
            string varName = dialogScript.InstStrings[nameIndex];

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

        VarType GetFuncReturnType(IndexedArray<ushort> indexedArray)
        {
            ushort nameIndex = indexedArray[indexedArray.Index + 1];
            string funcName = dialogScript.InstStrings[nameIndex];

            if (!DialogBridgeRegister.Methods.TryGetValue(funcName, out FuncDef? funcDef))
                return default;

            return funcDef.ReturnType;
        }
    }

    public static float GetFloatInstResult(
        DialogScript script,
        IStorageContext storage,
        IndexedArray<ushort> indexedArray)
    {
        SetPrivateFields(script, storage, indexedArray);
        float result = EvalFloatExp(false);
        ResetPrivateFields();
        return result;
    }

    public static bool GetBoolInstResult(
        DialogScript script,
        IStorageContext storage,
        IndexedArray<ushort> indexedArray)
    {
        SetPrivateFields(script, storage, indexedArray);
        bool result = EvalBoolExp(false);
        ResetPrivateFields();
        return result;
    }

    public static string GetStringInstResult(
        DialogScript script,
        IStorageContext storage,
        IndexedArray<ushort> indexedArray)
    {
        SetPrivateFields(script, storage, indexedArray);
        string result = EvalStringExp(false);
        ResetPrivateFields();
        return result;
    }

    public static void EvalVoidExp(
        DialogScript script,
        IStorageContext storage,
        IndexedArray<ushort> indexedArray)
    {
        SetPrivateFields(script, storage, indexedArray);

        switch ((OpCode)s_indexedArray.Current)
        {
            case OpCode.Assign:
                EvalAssign();
                break;
            case OpCode.MultAssign:
            case OpCode.DivAssign:
            case OpCode.AddAssign:
            case OpCode.SubAssign:
                EvalMathAssign((OpCode)s_indexedArray.Current);
                break;
            case OpCode.Func:
                EvalFunc();
                break;
        };

        ResetPrivateFields();

        return;

        void EvalAssign()
        {
            s_indexedArray.Index++;
            string varName = s_script.InstStrings[s_indexedArray.Current];

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
                s_indexedArray.Index++;
                switch (GetReturnType(s_script, s_storage, s_indexedArray))
                {
                    case VarType.Float:
                        s_storage.SetValue(varName, EvalFloatExp(false));
                        break;
                    case VarType.Bool:
                        s_storage.SetValue(varName, EvalBoolExp(false));
                        break;
                    case VarType.String:
                        s_storage.SetValue(varName, EvalStringExp(false));
                        break;
                }
            }
        }

        void EvalMathAssign(OpCode instructionType)
        {
            s_indexedArray.Index++;
            string varName = s_script.InstStrings[s_indexedArray.Current];

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
        IndexedArray<ushort> indexedArray,
        IStorageContext storage)
    {
        indexedArray.Index++;
        string speakerId = script.SpeakerIds[indexedArray.Current];
        string? displayName = null, portraitId = null, mood = null;
        indexedArray.Index++;

        while (indexedArray.Index < indexedArray.Array.Length)
        {
            switch ((OpCode)indexedArray.Current)
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
            indexedArray.Index++;
        }

        return new SpeakerUpdate(speakerId, displayName, portraitId, mood);

        string GetUpdateValue()
        {
            indexedArray.Index++;
            ushort[] updateInst = script.Instructions[indexedArray.Current];
            return GetStringInstResult(script, storage, new IndexedArray<ushort>(updateInst));
        }
    }

    private static void SetPrivateFields(DialogScript script, IStorageContext storage, IndexedArray<ushort> indexedArray)
    {
        s_script = script;
        s_storage = storage;
        s_indexedArray = indexedArray;
    }

    private static void ResetPrivateFields()
    {
        s_script = null!;
        s_storage = null!;
        s_indexedArray = s_defaultIndexedArray!;
    }

    private static bool EvalBoolExp(bool moveNext = true)
    {
        if (moveNext)
            s_indexedArray.Index++;

        return (OpCode)s_indexedArray.Current switch
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
            s_indexedArray.Index++;
            return s_indexedArray.Current == 1;
        }

        bool EvalEquals()
        {
            s_indexedArray.Index++;
            return GetReturnType(s_script, s_storage, s_indexedArray) switch
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
            s_indexedArray.Index++;

        return (OpCode)s_indexedArray.Current switch
        {
            OpCode.String => EvalString(),
            OpCode.Var => EvalVar<string>() ?? string.Empty,
            OpCode.Func => (string?)EvalFunc() ?? string.Empty,
            _ => string.Empty
        };

        string EvalString()
        {
            s_indexedArray.Index++;
            return s_script.InstStrings[s_indexedArray.Current];
        }
    }

    private static float EvalFloatExp(bool moveNext = true)
    {
        if (moveNext)
            s_indexedArray.Index++;

        return (OpCode)s_indexedArray.Current switch
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
            s_indexedArray.Index++;
            return s_script.InstFloats[s_indexedArray.Current];
        }

        float EvalMult() => EvalFloatExp() * EvalFloatExp();

        float EvalDiv() => EvalFloatExp() / EvalFloatExp();

        float EvalAdd() => EvalFloatExp() + EvalFloatExp();

        float EvalSub() => EvalFloatExp() - EvalFloatExp();
    }

    private static T? EvalVar<T>()
    {
        s_indexedArray.Index++;
        string varName = s_script.InstStrings[s_indexedArray.Current];

        if (DialogBridgeRegister.Properties.TryGetValue(varName, out VarDef? varDef))
            return ((Func<T>)varDef.Getter).Invoke();

        if (!s_storage.TryGetValue(varName, out object? obj) || obj is not T)
            return default;

        return (T)obj;
    }

    private static object? EvalFunc()
    {
        s_indexedArray.Index++;
        string funcName = s_script.InstStrings[s_indexedArray.Current];

        if (!DialogBridgeRegister.Methods.TryGetValue(funcName, out FuncDef? funcDef))
            return default;

        s_indexedArray.Index++;
        int argNum = s_indexedArray.Current;

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
