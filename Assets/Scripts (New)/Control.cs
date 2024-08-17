using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Control : MonoBehaviour {
    //This list stores all the players (human and AI) involved in the game.
    List<PlayerInterface> players = new List<PlayerInterface>();
    //These lists represent the deck of cards and the discard pile.
    public static List<Card> deck = new List<Card>();


    //public static List<Card> ingredientDeck = new List<Card>();
    //public static List<Card> actionDeck = new List<Card>();


    public static List<Card> discard = new List<Card>();
    // GameObject variables are used to reference UI elements, card prefabs, player hands, AI players, and the discard pile.
    public GameObject playerHand;
	public GameObject contentHolder;
	public Text dialogueText;

	public static GameObject discardPileObj;

	public GameObject regCardPrefab;
	public GameObject skipCardPrefab;
	public GameObject revrsCardPrefab;
	public GameObject drawCardPrefab;
	public GameObject wildCardPrefab;

	public GameObject[] colors = new GameObject[4];
	string[] colorsMatch = new string[4]{"Yellow","Green","Blue","Red"};
	public GameObject[] aiPlayers = new GameObject[5];
	public GameObject colorText;
	public GameObject deckGO;
	public GameObject pauseCan;
	public GameObject endCan;
	bool enabledStat=false;
    //These variables manage the current player's turn, the timer between AI moves, and whether the turn order is reversed.
    int where =0;
	float timer=0;
	bool reverse=false;
    //The number of AI players in the game.
    public static int numbOfAI;

	void Start () { //this does all the setup. Makes the human and ai players. sets the deck and gets the game ready
        //Both the deck and discard pile are cleared to start fresh.
        discard.Clear ();
		deck.Clear ();
        //The human player is added first, followed by AI players.
        players.Add (new HumanPlayer ("You"));
		for (int i = 0; i < numbOfAI; i++) {
			players.Add (new AiPlayer ("AI "+(i+1)));
		}
        //The human player is added first, followed by AI players.
        for (int i = 0; i < players.Count - 1; i++) {
			aiPlayers [i].SetActive (true);
			aiPlayers [i].transform.Find ("Name").GetComponent<Text> ().text = players [i + 1].getName ();
		}
        //A loop creates 15 different types of cards (UNO has cards numbered 0-9, special cards like Skip,
        //Reverse, Draw Two, and Wild cards). Each card is added to the deck with its respective prefab.
        for (int i = 0; i < 15; i++) { //setups the deck by making cards
			for (int j = 0; j < 8; j++) {
				switch (i) {
					case 10:
						deck.Add (new Card (i, returnColorName (j%4), skipCardPrefab));
						break;
					case 11:
						deck.Add (new Card (i, returnColorName (j%4), revrsCardPrefab));
						break;
					case 12:
						deck.Add (new Card (i, returnColorName (j%4), drawCardPrefab));
						break;
					case 13:
						deck.Add (new Card (i, "Black", wildCardPrefab));
						break;
					case 14:
						deck.Add (new Card (i, "Black", wildCardPrefab));
						break;
					default:
						deck.Add (new Card (i, returnColorName (j%4), regCardPrefab));
						break;						
				}

				if ((i == 0 || i>=13) && j >= 3)
					break;
			}
		}
        //The deck is shuffled to randomize the card order.
        shuffle ();
        //The first card is chosen from the deck and placed on the discard pile. 
        //If the first card is a special card (>= 10), the loop continues until a regular card (< 10) is found.
        Card first = null;
		if (deck [0].getNumb () < 10) {
			first = deck [0];
		}
		else {
			while (deck [0].getNumb () >= 10) {
				deck.Add (deck [0]);
				deck.RemoveAt (0);
			}
			first = deck [0];
		}
		discard.Add (first);
		discardPileObj = first.loadCard (725, -325, GameObject.Find ("Main").transform);
		deck.RemoveAt (0);
        //Each player is dealt 5 cards from the deck.

        foreach (PlayerInterface x in players) {
			for (int i = 0; i < 5; i++) {
				x.addCards (deck [0]);
				deck.RemoveAt (0);
			}
		}
	}
    //This method returns the name of a color (Green, Blue, Red, Yellow) based on an integer input.
    string returnColorName (int numb) { //returns a color based on a number, used in setup
		switch(numb) {
		case 0: 
			return "Green";
		case 1:
			return "Blue";
		case 2: 
			return "Red";
		case 3: 
			return "Yellow";
		}
		return "";
	}
    //This method shuffles the deck by swapping each card with another random card in the deck.
    void shuffle() { //shuffles the deck by changing cards around
		for (int i = 0; i < deck.Count; i++) {
			Card temp = deck.ElementAt (i);
			int posSwitch = Random.Range (0, deck.Count);
			deck [i] = deck [posSwitch];
			deck [posSwitch] = temp;
		}
	}
    //This method updates the dialogue box in the UI with the provided text.
    public void recieveText(string text) { //updates the dialogue box
		dialogueText.text += text + "\n";
		contentHolder.GetComponent<RectTransform> ().localPosition = new Vector2 (0, contentHolder.GetComponent<RectTransform> ().sizeDelta.y);
	}
    //This method updates the top card of the discard pile when a new card is played.
    public void updateDiscPile(Card card) { //this changes the last card played. Top of the discard pile
		discard.Add (card);
		Destroy(discardPileObj);
		discardPileObj=card.loadCard (725, -325, GameObject.Find ("Main").transform);
		discardPileObj.transform.SetSiblingIndex(9);
	}
    //This method updates the UI to show how many cards each AI player has left. It also checks if any player has won (i.e., has no cards left).
    public bool updateCardsLeft() { //this updates the number below each ai, so the player knows how many cards they have left
		for (int i = 0; i < players.Count - 1; i++) {
			int temp = players [i + 1].getCardsLeft ();
			aiPlayers [i].transform.Find ("CardsLeft").GetComponent<Text> ().text = temp.ToString();
		}
		foreach (PlayerInterface i in players) {
			if (i.getCardsLeft()==0) {
				this.enabled = false;
				recieveText (string.Format ("{0} won!", i.getName()));
				endCan.SetActive (true);
				endCan.transform.Find ("WinnerTxt").gameObject.GetComponent<Text> ().text = string.Format ("{0} Won!", i.getName ());
				return true;
			}
		}
		return false;
	}
    //This method handles the main game loop, alternating turns between the human player and AI players:
    void Update () { //this runs the players turns
		bool win = updateCardsLeft ();
		if (win)//The game checks if any player has won.
            return;
        //If it's the human player's turn, the game waits for player input. If the player is skipped, the next player's turn starts.
        if (players [where] is HumanPlayer)
        {
			if (players [where].skipStatus) {
				players [where].skipStatus = false;
				where += reverse ? -1 : 1;
				if (where >= players.Count)
					where = 0;
				else if (where < 0)
					where = players.Count - 1;
				return;
			}
			this.enabled = false;
			PlayerInterface temp = players [where];
			deckGO.GetComponent<Button> ().onClick.RemoveAllListeners ();
			deckGO.GetComponent<Button> ().onClick.AddListener (() => {
				draw (1, temp);
				((HumanPlayer)temp).recieveDrawOnTurn();
			});
			where+=reverse?-1:1;
			players [where+(reverse?1:-1)].turn ();
		}
        //If it's an AI player's turn, a timer is used to simulate thinking time before the AI makes a move.
        else if (players [where] != null) {
			if (players [where].skipStatus) {
				players [where].skipStatus = false;
				where += reverse ? -1 : 1;
				if (where >= players.Count)
					where = 0;
				else if (where < 0)
					where = players.Count - 1;
				return;
			}
			timer += Time.deltaTime;
			if (timer < 2.2)
				return;
			this.enabled = false;
			timer = 0;
			where+=reverse?-1:1;
			players [where+(reverse?1:-1)].turn ();
		}
		else
			where += reverse ? -1 : 1;
	
		if (where >= players.Count)
			where = 0;
		else if (where < 0)
			where = players.Count - 1;
			
	}
    //This method is triggered when a player plays a Wild card. It activates the UI elements that allow the player to choose a new color.
    public void startWild(string name) { //this starts the color chooser for the player to choose a color after playing a  wild
		for (int i = 0; i < 4; i++) {
			colors [i].SetActive (true);
			addWildListeners (i, name);
		}
		colorText.SetActive (true);
	}
    //This method adds listeners to the Wild card color buttons so that when a player selects a color, the game updates the discard pile and resumes.
    public void addWildListeners(int i, string name) { //this is ran from the start wild. It sets each color option as a button and sets the onclick events
		colors [i].GetComponent<Button> ().onClick.AddListener (() => {
			discard[discard.Count-1].changeColor(colorsMatch[i]);
			recieveText(string.Format("{0} played a wild, Color: {1}",name,colorsMatch[i]));

			Destroy(discardPileObj);
			discardPileObj=discard[discard.Count-1].loadCard (725, -325, GameObject.Find ("Main").transform);
			discardPileObj.transform.SetSiblingIndex(9);
			 
			foreach (GameObject x in colors) {
				x.SetActive (false);
				x.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			colorText.SetActive (false);
			this.enabled=true;
		});
	}
    //This method allows a player to draw a certain number of cards from the deck.
    public void draw(int amount, PlayerInterface who) { //gives cards to the players. Players can ask to draw or draw will actrivate from special cards
		if (deck.Count < amount) {
			resetDeck ();
		}
		for (int i = 0; i < amount; i++) {
			who.addCards (deck [0]);
			deck.RemoveAt (0);
		}
	}
    //This method is triggered when the deck runs out of cards. 
    //It resets the deck by shuffling the discard pile back into the deck, leaving the top card in the discard pile.
    public void resetDeck() { //this resets the deck when all of the cards run out
		print ("reseting");
		foreach (Card x in discard) {
			if (x.getNumb () == 13 || x.getNumb () == 14) {
				x.changeColor ("Black");
			}
			deck.Add (x);
		}
		shuffle ();
		Card last = discard [discard.Count - 1];
		discard.Clear ();
		discard.Add (last);
	}
    //This method handles the effects of special cards (Skip, Reverse, Draw Two, Wild Draw Four) when they are played.
    public void specialCardPlay(PlayerInterface player, int cardNumb) { //takes care of all special cards played
		int who = players.FindIndex (e=>e.Equals(player)) + (reverse?-1:1);
		if (who >= players.Count)
			who = 0;
		else if (who < 0)
			who = players.Count - 1;
		
		switch (cardNumb) {
			case 10:				
				players [who].skipStatus = true;
				break;
			case 11:
				reverse = !reverse;
				int difference = 0;
				if (reverse) {
					difference = who - 2;
					if (difference >= 0)
						where = difference;
					else {
						difference = Mathf.Abs (difference);
						where = players.Count - difference;
					}
				}
				else {
					difference = who + 2;
					if (difference > players.Count - 1)
						where = difference - players.Count;
					else
						where = difference;
				}
				break;
			case 12:
				draw (2, players [who]);
				break;
			case 14:
				draw (4, players [who]);
				break;
		}
		if(cardNumb!=14)
			this.enabled = true;
	}
	public void pause(bool turnOnOff) { //turns the pause canvas on/off
		if (turnOnOff) {
			pauseCan.SetActive (true);
			enabledStat = this.enabled;
			this.enabled = false;
		}
		else {
			pauseCan.SetActive (false);
			this.enabled = enabledStat;
		}
	}
    //These methods handle navigating back to the home screen, quitting the game, and resetting the game after it ends, respectively.
    public void returnHome() { //loads the home screen
		UnityEngine.SceneManagement.SceneManager.LoadScene ("Start");
	}
	public void exit() { //quits the app
		Application.Quit ();
	}
	public void playAgain() { //resets everything after a game has been played
		this.enabled = false;
		reverse = false;
		players.Clear ();
		dialogueText.text = "";
		contentHolder.GetComponent<RectTransform> ().localPosition = new Vector2 (0, 0);
		endCan.SetActive (false);
		for (int i = playerHand.transform.childCount - 1; i >= 0; i--) {
			Destroy (playerHand.transform.GetChild (i).gameObject);
		}
		Destroy(discardPileObj);
		where = 0;
		Start ();
		this.enabled = true;
	}
}
