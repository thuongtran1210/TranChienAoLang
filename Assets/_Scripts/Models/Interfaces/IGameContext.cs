using UnityEngine;
using System.Collections;

public interface IGameContext
{
    Coroutine StartCoroutine(IEnumerator routine);
    void EndSetupPhase(); 
    void EndGame(bool playerWon); 
}