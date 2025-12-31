using ET.Module;
using ET.Module.ETInput;
using UnityEngine;

public class InputChangeUI : MonoBehaviour
{
    public Transform changeSingleInputUI;
    public InputChangeUIConfirmDuplicate changeSingleInputUIConfirmDuplicate;
    bool _isChangingInput = false;
    bool _isChangingInput_ConfirmDuplicate = false;
    InputCode _currentChangingInputCode;
    KeyCode _curNewKeyCode;
    InputManager _inputManager;

    public void StartChangeInput(InputCode inputCode)
    {
        _currentChangingInputCode = inputCode;
        changeSingleInputUI.gameObject.SetActive(true);
    }
    public void StartChangeInput_ConfirmDuplicate(InputCode duplicateCode)
    {
        changeSingleInputUI.gameObject.SetActive(false);
        changeSingleInputUIConfirmDuplicate.gameObject.SetActive(true);
    }
    public void EndChangeInput()
    {
        _isChangingInput = false;
        _isChangingInput_ConfirmDuplicate = false;
        changeSingleInputUI.gameObject.SetActive(false);
        changeSingleInputUIConfirmDuplicate.gameObject.SetActive(false);
        changeSingleInputUIConfirmDuplicate.SetConflictInput(_curNewKeyCode.ToString(), _currentChangingInputCode.ToString());
    }


    void OnGUI()
    {
        if (_isChangingInput && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                EndChangeInput();
            }
            else
            {
                InputCode curInputCode = InputCode.Right;
                if(_inputManager.CheckKeyCodeExits(ref curInputCode,Event.current.keyCode))
                {
                    _isChangingInput_ConfirmDuplicate = true;
                    _curNewKeyCode = Event.current.keyCode;
                    StartChangeInput_ConfirmDuplicate(curInputCode);
                }
            }
        }
        if (_isChangingInput && _isChangingInput_ConfirmDuplicate && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                EndChangeInput();
            }
            else if (Event.current.keyCode == KeyCode.Return)
            {
                _inputManager.ChangeInputCode(_currentChangingInputCode, _curNewKeyCode);
            }
        }
    }
}
