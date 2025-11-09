using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame() => SceneManager.LoadScene("fase F"); 
    public void LoadStart() => SceneManager.LoadScene("TelaInicial");
    public void LoadHistory() => SceneManager.LoadScene("TelaHistoria");
    public void LoadInstruction() => SceneManager.LoadScene("TelaInstrucoes");
    public void LoadVictory() => SceneManager.LoadScene("Victory");
    public void LoadGameOver() => SceneManager.LoadScene("GameOver");
    public void Quit() => Application.Quit();
}

