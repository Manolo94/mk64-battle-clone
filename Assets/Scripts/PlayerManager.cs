using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public Transform[] PlayerStartingPositions;
    public string[] PlayerInputs;
    public GameObject Kart;

    private Player[] players;

	// Use this for initialization
	void Start () {
        // Initialize player list
        players = new Player[PlayerInputs.Length];
        for (int i = 0; i < players.Length; i++)
            players[i] = new Player { input = PlayerInputs[i], active = false, Kart = null };
	}
	
	// Update is called once per frame
	void Update () {
        // Check if someone joined the game and spawn them
        for (int id = 0; id < PlayerInputs.Length; id++)
        {
            //Debug.Log("players " + id + " " + players[id] == null ? " null" : " not null");
            if (Input.GetButton(PlayerInputs[id] + " accel") && players[id] != null && !players[id].active)
            {
                SpawnNewPlayer(id);

                // Assign viewport space based on the number of players
                int activePlayers = 0;
                for (int i = 0; i < players.Length; i++)
                    activePlayers += players[i].active ? 1 : 0;

                float left = 0;
                float top = 0;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].active)
                    {
                        float right = left + ((activePlayers > 1) ? 1 : 2) * 0.5f;
                        float bot = top + ((activePlayers > 2) ? 1 : 2) * 0.5f;

                        // Modify the viewport rects
                        Camera c = players[i].Kart.GetComponent<KartControl>().GetKartCamera();
                        c.rect = new Rect(left, top, right - left, bot - top);

                        // If the right border has been reached, move to the next row
                        if (right == 1.0)
                        {
                            left = 0;
                            top = bot;
                        }
                        else
                        {
                            left = right;
                        }
                    }
                }
            }
        }

	}

    void SpawnNewPlayer(int id)
    {
        // Create a new Kart and add it to the list of players
        players[id].Kart = GameObject.Instantiate<GameObject>(Kart, PlayerStartingPositions[id]);

        // Assign input
        players[id].Kart.GetComponent<KartControl>().InputName = players[id].input;

        // Activate the player
        players[id].active = true;
    }

    public class Player
    {
        public string input;
        public bool active;
        public GameObject Kart;

        private int Lives;

        private Item Inventory;
        private Item Standby;
        private Item Active;

        public void Send_Input_To_Active(Input input)
        {
            Active.Process_Input(input);
        }

        /* This should be called:
             1. whenever an Item goes from On_Standby to In_World
             2. whenever a new Item gets added to Inventory */
        public void Update_Active_Item()
        {
            if (Active == null)
                Active = Inventory;
        }

        public void Add_New_Item_To_Inventory()
        {
            if (Inventory == null)
                Inventory = Get_Random_Item();

            Update_Active_Item();
        }

        // TODO: Implement this
        private Item Get_Random_Item()
        {
            return null;
        }

        public void Clear_Active_Item()
        {
            Active = null;
        }
        public void Clear_Inventory_Item()
        {
            Inventory = null;
        }
        public void Clear_Standby_Item()
        {
            Standby = null;
        }
        public void Move_From_Inventory_To_Standby()
        {
            if (Standby == null)
            {
                Standby = Inventory;
                Clear_Inventory_Item();
            }
            else
            {
                throw new System.Exception("Attempting to move an Item to Standby, but Standby is not empty");
            }
        }
    }

    public abstract class Item
    {
        protected enum State { In_Inventory, In_Container, On_Standby, In_World };

        protected State state;
        protected Player owner;
        protected GameObject itemGameObject;

        public abstract void Process_Input(Input input);

        void Move_To_State(State newState)
        {
            if (state == State.In_Inventory && newState == State.On_Standby)
                owner.Move_From_Inventory_To_Standby();
            else if(state == State.On_Standby && newState == State.In_World)
            {
                owner.Clear_Standby_Item();
                owner.Clear_Active_Item();
                owner.Update_Active_Item();
            }

            Exiting_State();
            state = newState;
            Entering_State();
        }

        protected abstract void Entering_State();
        protected abstract void Exiting_State();

        protected abstract void Update();
    }
}
