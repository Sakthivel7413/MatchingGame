using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerScript : MonoBehaviour
{
    public const int columns = 9;
    public const int rows = 5;

    public const float Xspace = 3f;
    public const float Yspace = -3f;

    [SerializeField] private MainImageScript startObject;
    [SerializeField] private Sprite[] images;

    private const int maxAttempts = 25; 

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [SerializeField] private TextMeshProUGUI attemptsText; 
    [SerializeField] private TextMeshProUGUI gameOverText; 
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private GameObject restartButton; 

    private MainImageScript firstOpen;
    private MainImageScript secondOpen;

    private int score = 0;
    private int attempts;
    private int totalCards; 
    private int matchedPairs;
    private int highScore;

    private void Start()
    {
      
        attempts = maxAttempts;
        UpdateAttemptText(); 
        UpdateScoreText();  
        gameOverText.gameObject.SetActive(false); 
        winText.gameObject.SetActive(false); 
        restartButton.SetActive(false);


        highScore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScoreText();

        int totalCells = columns * rows;
        totalCards = totalCells;
        matchedPairs = 0;

        List<int> locations = new List<int>();

        for (int i = 0; i < totalCells / 2; i++)
        {
            locations.Add(i);
            locations.Add(i);
        }

        int[] randomizedLocations = Randomiser(locations.ToArray());

        Vector3 startPosition = startObject.transform.position;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                MainImageScript gameImage;
                if (i == 0 && j == 0)
                {
                    gameImage = startObject;
                }
                else
                {
                    gameImage = Instantiate(startObject) as MainImageScript;
                }

                int index = j * columns + i;

                if (index < randomizedLocations.Length)
                {
                    int id = randomizedLocations[index];
                    if (id < images.Length)
                    {
                        gameImage.ChangeSprite(id, images[id]);
                    }
                }

                float positionX = (Xspace * i) + startPosition.x;
                float positionY = (Yspace * j) + startPosition.y;

                gameImage.transform.position = new Vector3(positionX, positionY, startPosition.z);
            }
        }
    }

   

    private int[] Randomiser(int[] locations)
    {
        int[] array = locations.Clone() as int[];
        for (int i = 0; i < array.Length; i++)
        {
            int newArray = array[i];
            int j = Random.Range(i, array.Length);
            array[i] = array[j];
            array[j] = newArray;
        }
        return array;
    }

    public bool canOpen
    {
        get { return secondOpen == null && attempts > 0; } 
    }

    public void imageOpened(MainImageScript startObject)
    {
        if (firstOpen == null && attempts > 0)
        {
            firstOpen = startObject;
        }
        else if (secondOpen == null && attempts > 0)
        {
            secondOpen = startObject;
            StartCoroutine(CheckGuessed());
        }
    }

    private IEnumerator CheckGuessed()
    {
        if (firstOpen.spriteId == secondOpen.spriteId) 
        {
            score++; 
            UpdateScoreText();

            
            yield return new WaitForSeconds(0.5f);
            firstOpen.gameObject.SetActive(false);
            secondOpen.gameObject.SetActive(false);

            IncreaseAttempts();
            matchedPairs++;

            
            if (matchedPairs >= totalCards / 2)
            {
                Win(); 
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f); 

            firstOpen.Close();
            secondOpen.Close();
        }

        attempts--;
        UpdateAttemptText(); 

        
        if (attempts <= 0)
        {
            GameOver();
        }

        firstOpen = null;
        secondOpen = null;
    }

    private void IncreaseAttempts()
    {
        if (attempts < maxAttempts)
        {
            attempts++;
            UpdateAttemptText(); 
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    private void UpdateAttemptText()
    {
        attemptsText.text = "Attempts: " + attempts;
    }




    private void UpdateHighScoreText()
    {
        highScoreText.text = "High Score: " + highScore;
    }

    private void CheckAndUpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); 
            UpdateHighScoreText(); 
        }
    }
    private void Win()
    {
        
        winText.text = "You Win!";
        winText.gameObject.SetActive(true); 

       
        restartButton.SetActive(true);

        
        DisableAllCards();


        CheckAndUpdateHighScore();
    }

    private void GameOver()
    {
        
        gameOverText.text = "Game Over!";
        gameOverText.gameObject.SetActive(true);

        
        restartButton.SetActive(true);

       
        DisableAllCards();

        CheckAndUpdateHighScore();
    }

    private void DisableAllCards()
    {
        MainImageScript[] allCards = FindObjectsOfType<MainImageScript>();
        Debug.Log($"Found {allCards.Length} cards to disable.");

        foreach (var card in allCards)
        {
            Debug.Log($"Disabling card: {card.gameObject.name}");

            
            Collider2D collider = card.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

          
            card.enabled = false;

            
            card.gameObject.SetActive(false);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene("Scene7");
    }
}