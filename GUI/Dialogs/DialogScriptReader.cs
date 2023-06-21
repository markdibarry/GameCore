using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCore.Utility;

namespace GameCore.GUI;

public class DialogScriptReader
{
    public DialogScriptReader(Dialog dialog, DialogScript dialogScript)
    {
        _dialog = dialog;
        _dialogScript = dialogScript;
        _speakers = _dialogScript.SpeakerIds.Select(id => new Speaker(id)).ToArray();
    }

    private readonly Dialog _dialog;
    private readonly DialogScript _dialogScript;
    private readonly Speaker[] _speakers;
    private readonly IStorageContext _textStorage = new TextStorage();
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
            string translatedText = _dialog.TrS(lineData.Text);
            DialogLine line = new(_dialogScript, _textStorage, lineData, translatedText, _speakers, _autoGlobal);
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
            ushort[] instValues = _dialogScript.Instructions[conditions.Index];

            foreach (ushort instValue in instValues)
            {
                InstructionStatement condition = _dialogScript.InstructionStmts[instValue];
                var enumerator = _dialogScript.Instructions[condition.Index].GetEnumerator<ushort>();

                if (DialogInterpreter.GetBoolInstResult(_dialogScript, _textStorage, enumerator))
                {
                    await ReadNextStatementAsync(condition.Next);
                    return;
                }
            }

            await ReadNextStatementAsync(conditions.Next);
        }

        async Task HandleChoiceStatement()
        {
            await _dialog.OpenOptionBoxAsync(GetChoices());

            List<Choice> GetChoices()
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
                        i++;
                        Choice choice = _dialogScript.Choices[choiceSet[i]];
                        choice.Disabled = i < validIndex;
                        choices.Add(choice);
                        continue;
                    }

                    // If GoTo, flag all choices as disabled until index
                    if (choiceSet[i] == (ushort)OpCode.Goto)
                    {
                        i++;
                        if (validIndex == -1)
                            validIndex = choiceSet[i];
                        continue;
                    }

                    // Otherwise is a condition
                    if (i < validIndex)
                    {
                        i++;
                        continue;
                    }

                    i++;
                    ushort[] condition = _dialogScript.Instructions[choiceSet[i]];
                    IEnumerator<ushort> enumerator = condition.GetEnumerator<ushort>();

                    if (!DialogInterpreter.GetBoolInstResult(_dialogScript, _textStorage, enumerator))
                        validIndex = choiceSet[i];
                }
                return choices;
            };
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
            var enumerator = instructions.GetEnumerator<ushort>();
            VarType returnType = DialogInterpreter.GetReturnType(_dialogScript, _textStorage, enumerator);
            enumerator.MoveNext();

            switch (returnType)
            {
                case VarType.String:
                    DialogInterpreter.GetStringInstResult(_dialogScript, _textStorage, enumerator);
                    break;
                case VarType.Float:
                    DialogInterpreter.GetFloatInstResult(_dialogScript, _textStorage, enumerator);
                    break;
                case VarType.Bool:
                    DialogInterpreter.GetBoolInstResult(_dialogScript, _textStorage, enumerator);
                    break;
                case VarType.Void:
                    DialogInterpreter.EvalVoidExp(_dialogScript, _textStorage, enumerator);
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
            var enumerator = instructions.GetEnumerator<ushort>();
            SpeakerUpdate speakerUpdate = DialogInterpreter.GetSpeakerUpdate(_dialogScript, enumerator, _textStorage);
            _dialog.UpdateSpeaker(true,
                speakerUpdate.SpeakerId,
                speakerUpdate.DisplayName,
                speakerUpdate.PortraitId,
                speakerUpdate.Mood);
        }
    }
}
