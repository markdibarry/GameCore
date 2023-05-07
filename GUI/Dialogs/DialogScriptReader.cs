using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore.GUI;

public class DialogScriptReader
{
    public DialogScriptReader(Dialog dialog, DialogBridgeRegister register, DialogScript dialogScript)
    {
        _dialog = dialog;
        _dialogScript = dialogScript;
        _speakers = _dialogScript.SpeakerIds.Select(id => new Speaker(id)).ToArray();
        _interpreter = new(register, dialogScript, new());
    }

    private readonly DialogInterpreter _interpreter;
    private readonly Dialog _dialog;
    private readonly DialogScript _dialogScript;
    private readonly Speaker[] _speakers;
    private bool _autoGlobal = false;

    public void Evaluate(ushort[] instructions)
    {
        EvaluateInstructions(instructions);
    }

    public Speaker GetSpeaker(string speakerId)
    {
        return _speakers.First(x => x.SpeakerId == speakerId);
    }

    public async Task ReadScriptAsync()
    {
        await ReadNextStatementAsync(_dialogScript.Sections[0].Next);
    }

    public async Task ReadNextStatementAsync(GoTo goTo)
    {
        switch (goTo.Type)
        {
            case StatementType.Line:
                await HandleLineStatement();
                break;
            case StatementType.Section:
                await HandleSectionStatement();
                break;
            case StatementType.Instruction:
                await HandleInstructionStatement();
                break;
            case StatementType.Conditional:
                await HandleConditionalStatement();
                break;
            case StatementType.Choice:
                await HandleChoiceStatement();
                break;
            case StatementType.End:
            default:
                await HandleEnd();
                break;
        }

        async Task HandleLineStatement()
        {
            LineData lineData = _dialogScript.Lines[goTo.Index];
            DialogLine line = new(_dialog, _interpreter, _dialogScript, lineData, _speakers, _autoGlobal);
            await _dialog.HandleLineAsync(line);
        }

        async Task HandleSectionStatement()
        {
            await ReadNextStatementAsync(_dialogScript.Sections[goTo.Index].Next);
        }

        async Task HandleInstructionStatement()
        {
            InstructionStatement instructionStmt = _dialogScript.InstructionStmts[goTo.Index];
            GoTo next = instructionStmt.Next;
            if (instructionStmt.Index != -1)
            {
                next = EvaluateInstructions(_dialogScript.Instructions[instructionStmt.Index]);
                if (next.Type == StatementType.Undefined)
                    next = instructionStmt.Next;
            }
            await ReadNextStatementAsync(next);
        }

        async Task HandleConditionalStatement()
        {
            InstructionStatement conditions = _dialogScript.InstructionStmts[goTo.Index];
            ushort[] values = _dialogScript.Instructions[conditions.Index];
            for (int i = 0; i < values.Length; i++)
            {
                InstructionStatement condition = _dialogScript.InstructionStmts[values[i]];
                if (_interpreter.GetBoolInstResult(_dialogScript.Instructions[condition.Index]))
                {
                    await ReadNextStatementAsync(condition.Next);
                    return;
                }
            }
            await ReadNextStatementAsync(conditions.Next);
        }

        async Task HandleChoiceStatement()
        {
            List<Choice> choices = new();
            ushort[] choiceSet = _dialogScript.ChoiceSets[goTo.Index];
            int validIndex = -1;
            for (int i = 0; i < choiceSet.Length; i++)
            {
                if (i >= validIndex)
                    validIndex = -1;
                // If choice, add it!
                if (choiceSet[i] == (ushort)OpCode.Choice)
                {
                    Choice choice = _dialogScript.Choices[choiceSet[++i]];
                    choice.Disabled = i < validIndex;
                    choices.Add(choice);
                }
                // If GoTo, flag all choices as disabled until index
                else if (choiceSet[i] == (ushort)OpCode.Goto)
                {
                    if (validIndex == -1)
                        validIndex = choiceSet[++i];
                    else
                        i++;
                }
                // Otherwise is a condition
                else
                {
                    if (i < validIndex)
                    {
                        i++;
                    }
                    else
                    {
                        ushort[] condition = _dialogScript.Instructions[choiceSet[i++]];
                        if (!_interpreter.GetBoolInstResult(condition))
                            validIndex = choiceSet[i];
                    }
                }
            }
            await _dialog.OpenOptionBoxAsync(choices);
        }

        async Task HandleEnd() => await _dialog.CloseDialogAsync();
    }

    private GoTo EvaluateInstructions(ushort[] instructions)
    {
        switch ((OpCode)instructions[0])
        {
            case OpCode.Assign:
            case OpCode.MultAssign:
            case OpCode.DivAssign:
            case OpCode.AddAssign:
            case OpCode.SubAssign:
            case OpCode.Func:
                HandleEvaluate();
                break;
            case OpCode.Speed:
                HandleSpeed();
                break;
            case OpCode.Goto:
                return HandleGoTo();
            case OpCode.Auto:
                HandleAuto();
                break;
            case OpCode.SpeakerSet:
                HandleSpeakerSet();
                break;
        };

        return GoTo.Default;

        void HandleEvaluate()
        {
            switch (_interpreter.GetReturnType(instructions, 0))
            {
                case VarType.String:
                    _interpreter.GetStringInstResult(instructions);
                    break;
                case VarType.Float:
                    _interpreter.GetFloatInstResult(instructions).ToString();
                    break;
                case VarType.Bool:
                    _interpreter.GetBoolInstResult(instructions);
                    break;
                case VarType.Void:
                    _interpreter.EvalVoidInst(instructions);
                    break;
            }
        }

        void HandleSpeed()
        {
            _dialog.SpeedMultiplier = _dialogScript.InstFloats[instructions[1]];
        }

        GoTo HandleGoTo() => new(StatementType.Section, instructions[1]);

        void HandleAuto() => _autoGlobal = instructions[1] == 1;

        void HandleSpeakerSet()
        {
            int i = 1;
            string speakerId = _dialogScript.SpeakerIds[instructions[i++]];
            string? displayName = null, portraitId = null, mood = null;
            while (i < instructions.Length)
            {
                switch ((OpCode)instructions[i++])
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

            _dialog.UpdateSpeaker(true, speakerId, displayName, portraitId, mood);

            string GetUpdateValue()
            {
                ushort[] updateInst = _dialogScript.Instructions[instructions[i++]];
                return _interpreter.GetStringInstResult(updateInst);
            }
        }
    }
}
