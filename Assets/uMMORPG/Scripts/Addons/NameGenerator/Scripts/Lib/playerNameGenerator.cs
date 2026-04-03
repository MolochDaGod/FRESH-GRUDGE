using UnityEngine;
public class playerNameGenerator : MonoBehaviour
{

#if _iMMOCHARACTERCREATION
    public UI_CharacterCreation characterCreation;
#else
    public UICharacterCreation characterCreation;
#endif
    public ScriptablePlayerName scriptablePlayerName;
    public void GenName()
    {
#if _iMMOCHARACTERCREATION
        characterCreation.nameInput.text = GetName(characterCreation.currentPlayer.name);
#else
        characterCreation.nameInput.text = GetName(characterCreation.classDropdown.captionText.text);
#endif
    }

    private string GetName(string className)
    {
        foreach(var clase in scriptablePlayerName.playerPlassName)
        {
            if(clase.prefabPlayerName == className && clase.firstNamePart.Length > 0 && clase.secondNamePart.Length > 0 && clase.thirdNamePart.Length > 0   )
            {
                 return (
                    clase.firstNamePart[UnityEngine.Random.Range(0, clase.firstNamePart.Length)] + 
                    clase.secondNamePart[UnityEngine.Random.Range(0, clase.secondNamePart.Length)] + 
                    clase.thirdNamePart[UnityEngine.Random.Range(0, clase.thirdNamePart.Length)]);
            }
        }
    
        // not className find in list return Default list
        return (
            scriptablePlayerName.playerPlassName[0].firstNamePart[UnityEngine.Random.Range(0, scriptablePlayerName.playerPlassName[0].firstNamePart.Length)] +
            scriptablePlayerName.playerPlassName[0].secondNamePart[UnityEngine.Random.Range(0, scriptablePlayerName.playerPlassName[0].secondNamePart.Length)] +
            scriptablePlayerName.playerPlassName[0].thirdNamePart[UnityEngine.Random.Range(0, scriptablePlayerName.playerPlassName[0].thirdNamePart.Length)]);
    }
}