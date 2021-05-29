using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Cards;
using GraphicCards;
using Poker_Model;

namespace GraphicPoker_Model
{
    public class GraphicPokerGame : PokerGame
    {
        Timer DealCardsToPlayers_timer;
        Timer DoAutomaticBets_timer;
        Timer PauseBeetweenRounds_timer;

        int countOfTimesDealCardsToPlayers_timerWasCalled = 0;
        int countOfTimesDoAutomaticBets_timerWasCalled = 0;
        int countOfTimesPauseBeetweenRounds_timerWasCalled = 0;

        private Label bankValueLabel;
        public Label BankValueLabel
        {
            get
            {
                return bankValueLabel;
            }
        }

        private Button restartGameButton;
        public Button RestartGameButton
        {
            get
            {
                return restartGameButton;
            }
        }

        private Action<string> ShowMessage;

        int countCallButtonWasClickedAfterSomeoneDoAllInBet = 0;
        bool SomeoneDoAllInBet = false;

        public GraphicPokerGame(List<GraphicPokerPlayer> players, Panel tableCardsPanel, Panel deckPanel, Label gameBankValueLabel,
            Button restartGameButton, Timer dealCardsTimer, Timer pauseBeetweenRoundsTimer, Timer DoAutomaticBetsTimer,
            Action<string> ShowMessage)
            : base(new List<PokerPlayer>(players))
        {
            bankValueLabel = gameBankValueLabel;

            this.ShowMessage = ShowMessage;

            this.restartGameButton = restartGameButton;
            RestartGameButton.Click += new EventHandler(RestartGame_button_Click);

            DealCardsToPlayers_timer = dealCardsTimer;
            DealCardsToPlayers_timer.Tick += DealCardsToPlayers_timer_Tick;

            DoAutomaticBets_timer = DoAutomaticBetsTimer;
            DoAutomaticBets_timer.Tick += DoAutomaticBets_timer_Tick;

            PauseBeetweenRounds_timer = pauseBeetweenRoundsTimer;
            PauseBeetweenRounds_timer.Tick += PauseBeetweenRounds_timer_Tick;

            foreach (var player in Players)
            {
                GraphicPokerPlayer graphicPlayer = (GraphicPokerPlayer)player;
                graphicPlayer.FoldButton.Click += new EventHandler(Fold);
                graphicPlayer.CheckButton.Click += new EventHandler(Check);
                graphicPlayer.CallButton.Click += new EventHandler(Call);
                graphicPlayer.RaiseButton.Click += new EventHandler(MakeVisibleWriteSumOfRaiseMenu);
                graphicPlayer.SumOfRaiseTextBox.TextChanged += new EventHandler(EnterValueOfRaiseBet_ActivePlayerTextBox_TextChanged);
                graphicPlayer.ConfirmRaiseButton.Click += new EventHandler(ConfirmRaise);
                graphicPlayer.CancelRaiseButton.Click += new EventHandler(CancelRaiseMenu);
            }

            GraphicCardSet graphicDeck = (GraphicCardSet)Deck;
            graphicDeck.Panel = deckPanel;
            graphicDeck.Draw();

            GraphicCardSet graphicTable = (GraphicCardSet)CardsOnTable;
            graphicTable.Panel = tableCardsPanel;
            graphicTable.Draw();

            ShowPlayersBanksInStartOfGame();
        }

        private void RestartGame()
        {
            Restart();
            StartGame();
        }

        public void StartGame()
        {
            DealCardsToPlayers();
        }

        private void RestartGame_button_Click(object sender, EventArgs e)
        {
            RestartGameButton.Visible = false;
            RestartGame();
        }

        private void EnterValueOfRaiseBet_ActivePlayerTextBox_TextChanged(object sender, EventArgs e)
        {
            GraphicPokerPlayer graphicPlayer = (GraphicPokerPlayer)ActivePlayer;
            if (graphicPlayer.SumOfRaiseTextBox.Text != "")
            {
                graphicPlayer.ConfirmRaiseButton.Enabled = true;
            }
            else
            {
                graphicPlayer.ConfirmRaiseButton.Enabled = false;
            }
        }

        protected override CardSet GetDeck()
        {
            GraphicCardSet Deck = new GraphicCardSet(new Panel());
            Deck.Full();
            return Deck;
        }
        protected override CardSet GetTableCards()
        {
            return new GraphicCardSet(new Panel());
        }

        private void MakeTheActivePlayerActionSelectionMenuVisible()
        {
            GraphicPokerPlayer graphicActivePlayer = (GraphicPokerPlayer)ActivePlayer;
            if (CheckIfCanActivePlayerCheck())
            {
                foreach (var control in graphicActivePlayer.CheckModeControls)
                {
                    control.Visible = true;
                }
            }
            else
            {
                foreach (var control in graphicActivePlayer.CallModeControls)
                {
                    control.Visible = true;
                }
            }
        }

        public override void ChangeActivePlayer()
        {
            //Смени индекс активного игрока на индекс следующего игрока по списку игроков, тоесть :
            //если индекс активного игрока был индексом последнего игрока в списке игроков
            if (IndexOfActivePlayerInPlayersList == Players.Count - 1)
            {
                //то сделай индекс активного игрока равным индексу первого игрока в списке игроков, тоесть нулю.
                indexOfActivePlayerInPlayersList = 0;
            }
            //иначе же сделай индекс активного игрока равным индексу следующего игрока в списке игроков.
            else
            {
                indexOfActivePlayerInPlayersList++;
            }
            //Теперь сделай активным игрока с новым индексом
            activePlayer = Players[indexOfActivePlayerInPlayersList];

            ShowActivePlayerCards();

            if (CurrentRound != Round.First)
            {
                //Напоследок сделай меню выбора действий активного игрока видимым
                MakeTheActivePlayerActionSelectionMenuVisible();

                if (CanTheActivePlayerMakeCombination)
                {
                    CardSet activePlayerCardsAndTableCards = new CardSet();
                    activePlayerCardsAndTableCards.Add(activePlayer.HandCards);
                    activePlayerCardsAndTableCards.Add(CardsOnTable);

                    activePlayer.Combination = PokerCombination.GetCombination(activePlayerCardsAndTableCards);
                    ((GraphicPokerPlayer)activePlayer).CombinationValueLabel.Text = activePlayer.Combination.ToString();
                }
            }
            else
            {
                if(ActivePlayer != SmallBlindPlayer && ActivePlayer != BigBlindPlayer)
                {
                    MakeTheActivePlayerActionSelectionMenuVisible();

                    if (CanTheActivePlayerMakeCombination)
                    {
                        CardSet activePlayerCardsAndTableCards = new CardSet();
                        activePlayerCardsAndTableCards.Add(activePlayer.HandCards);
                        activePlayerCardsAndTableCards.Add(CardsOnTable);

                        activePlayer.Combination = PokerCombination.GetCombination(activePlayerCardsAndTableCards);
                        ((GraphicPokerPlayer)activePlayer).CombinationValueLabel.Text = activePlayer.Combination.ToString();
                    }
                }
            }
        }

        protected override void ShowActivePlayerCards()
        {
            base.ShowActivePlayerCards();
            ((GraphicCardSet)ActivePlayer.HandCards).Draw();
        }

        public override void NextRound()
        {
            base.NextRound();
            foreach (var player in Players)
            {
                ((GraphicPokerPlayer)player).BetValueLabel.Text = player.Bet.ToString();
            }
            /*game*/
            bankValueLabel.Text = Bank.ToString();
            ((GraphicCardSet)CardsOnTable).Draw();
        }

        public void Fold(object sender, EventArgs e)
        {
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;
            List<Control> playersActionControls;

            if (player.CombinationValueLabel.Visible)
            {
                player.CombinationValueLabel.Visible = false;
            }
            if (player.CallButton.Visible == true)
            {
                playersActionControls = player.CallModeControls;
            }
            else
            {
                playersActionControls = player.CheckModeControls;
            }

            foreach (var control in playersActionControls)
            {
                control.Visible = false;
            }
            if (player.SumOfRaiseTextBox.Visible == true)
            {
                foreach (var control in player.RaiseButttonMenuControls)
                {
                    control.Visible = false;
                }
            }

            HideActivePlayersCards();

            DeleteActivePlayerFromPlayersList();

            if (Players.Count == 1)
            {
                FoldWin();
                RestartGameButton.Visible = true;
            }

            if (countOfPlayersThatChooseToCheck == Players.Count)
            {
                countOfPlayersThatChooseToCheck = 0;
                PauseBeetweenRounds_timer.Start();
            }
            else
            {
                CheckIfGameOver(player);
            }
        }

        public void Check(object sender, EventArgs e)
        {
            countOfPlayersThatChooseToCheck++;

            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;

            if (player.CombinationValueLabel.Visible)
            {
                player.CombinationValueLabel.Visible = false;
            }
            foreach (var control in player.CheckModeControls)
            {
                control.Visible = false;
            }
            if (player.SumOfRaiseTextBox.Visible == true)
            {
                foreach (var control in player.RaiseButttonMenuControls)
                {
                    control.Visible = false;
                }
            }

            HideActivePlayersCards();

            ////////////////////////////
            //bool ifSomeoneDoAllBankBet = false;
            //foreach (var playyer in Players)
            //{
            //    if (playyer.Bank == 0)
            //    {
            //        ifSomeoneDoAllBankBet = true;
            //        break;
            //    }
            //}
            /////////////////////////////


            //First version
            //if (countOfPlayersThatChooseToCheck == Players.Count)
            //{
            //    countOfPlayersThatChooseToCheck = 0;
            //    PauseBeetweenRounds_timer.Start();
            //}
            //else
            //{
            //    CheckIfGameOver(player);
            //}

            //Second version
            /////////////////////
            //if (ifSomeoneDoAllBankBet)
            //{
            //    //////////////
            //    SendBetsOfPlayersToGameBank();
            //    BankValueLabel.Text = Bank.ToString();
            //    GoToLastRound();
            //    //////////////
            //}
            //else
            //{
                if (countOfPlayersThatChooseToCheck == Players.Count)
                {
                    countOfPlayersThatChooseToCheck = 0;
                    PauseBeetweenRounds_timer.Start();
                }
                else
                {
                    CheckIfGameOver(player);
                }
            //}
            /////////////////////
        }

        ////////////////////////////
        private void GoToLastRound()
        {
            PauseBeetweenRounds_timer.Tick -= PauseBeetweenRounds_timer_Tick;
            PauseBeetweenRounds_timer.Tick += GoToLastRound_Tick;

            PauseBeetweenRounds_timer.Start();
        }

        private void GoToLastRound_Tick(object sender, EventArgs e)
        {
            if (currentRound == Round.Fourth)
            {
                PauseBeetweenRounds_timer.Stop();
                GameOver();
                RestartGameButton.Visible = true;
            }
            else
            {
                DealCardsOnTable();
                currentRound++;

                if (currentRound == Round.Third)
                {
                    CardSet activePlayerCardsAndTableCards;
                    foreach (var player in Players)
                    {
                        activePlayerCardsAndTableCards = new CardSet();
                        activePlayerCardsAndTableCards.Add(CardsOnTable);
                        activePlayerCardsAndTableCards.Add(player.HandCards);
                        player.Combination = PokerCombination.GetCombination(activePlayerCardsAndTableCards);
                        ((GraphicPokerPlayer)player).CombinationValueLabel.Text = player.Combination.ToString();
                    }
                }
            }
        }
        ////////////////////////////

        protected override void HideActivePlayersCards()
        {
            base.HideActivePlayersCards();
            ((GraphicCardSet)ActivePlayer.HandCards).Draw();
        }

        private void CheckIfGameOver(GraphicPokerPlayer player)
        {
            if (!IsGameOver)
            {
                ChangeActivePlayer();
                player = (GraphicPokerPlayer)ActivePlayer;

                if (CheckIfCanActivePlayerCheck())
                {
                    foreach (var control in player.CheckModeControls)
                    {
                        control.Visible = true;
                    }
                }
                else
                {
                    foreach (var control in player.CallModeControls)
                    {
                        control.Visible = true;
                    }
                }
                if (CanTheActivePlayerMakeCombination)
                {
                    ShowActivePlayerCombination();
                }
            }
            else
            {
                RestartGameButton.Visible = true;
            }
        }

        public void Call(object sender, EventArgs e)
        {
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;

            if (player.CombinationValueLabel.Visible)
            {
                player.CombinationValueLabel.Visible = false;
            }
            foreach (var control in player.CallModeControls)
            {
                control.Visible = false;
            }
            if (player.SumOfRaiseTextBox.Visible == true)
            {
                foreach (var control in player.RaiseButttonMenuControls)
                {
                    control.Visible = false;
                }
            }

            HideActivePlayersCards();

            //First version
            //if (IndexOfActivePlayerInPlayersList != 0)
            //{
            //    ActivePlayer.Bet += Players[IndexOfActivePlayerInPlayersList - 1].Bet - ActivePlayer.Bet;
            //}
            //else
            //{
            //    ActivePlayer.Bet += Players[Players.Count - 1].Bet - ActivePlayer.Bet;
            //}

            //Second version
            ///////////////////////////////////
            if (BetOfTheRound <= ActivePlayer.Bank)
            {
                ActivePlayer.Bet += BetOfTheRound - ActivePlayer.Bet;
            }
            else
            {
                ActivePlayer.Bet += ActivePlayer.Bank;
            }
            ///////////////////////////////////

            player.BetValueLabel.Text = player.Bet.ToString();
            player.BankValueLabel.Text = player.Bank.ToString();

            if(player.Bank == 0)
            {
                SomeoneDoAllInBet = true;
            }

            ///////////////////
            if(SomeoneDoAllInBet)
            {
                countCallButtonWasClickedAfterSomeoneDoAllInBet++;
            }
            
            if (countCallButtonWasClickedAfterSomeoneDoAllInBet == 3)
            {
                SomeoneDoAllInBet = false;
                countCallButtonWasClickedAfterSomeoneDoAllInBet = 0;
                
                SendBetsOfPlayersToGameBank();
                BankValueLabel.Text = Bank.ToString();
                GoToLastRound();
            }
            //////////////////
            else
            {
                ChangeActivePlayer();
                player = (GraphicPokerPlayer)ActivePlayer;

                if (CheckIfCanActivePlayerCheck())
                {
                    foreach (var control in player.CheckModeControls)
                    {
                        control.Visible = true;
                    }
                }
                else
                {
                    foreach (var control in player.CallModeControls)
                    {
                        control.Visible = true;
                    }
                }
                if (CanTheActivePlayerMakeCombination)
                {
                    ShowActivePlayerCombination();
                }
            }
        }

        private void CancelRaiseMenu(object sender, EventArgs e)
        {
            MakeInVisibleWriteSumOfRaiseMenu();
        }

        public void MakeInVisibleWriteSumOfRaiseMenu() 
        {
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;
            player.CancelRaiseButton.Visible = false;
            player.ConfirmRaiseButton.Visible = false;
            player.SumOfRaiseTextBox.Visible = false;
            player.SumOfRaiseTextBox.Text = "";
            player.RaiseButton.Visible = true;
        }

        public void MakeVisibleWriteSumOfRaiseMenu(object sender, EventArgs e)
        {
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;
            player.RaiseButton.Visible = false;
            player.CancelRaiseButton.Visible = true;
            player.ConfirmRaiseButton.Visible = true;
            player.SumOfRaiseTextBox.Visible = true;

            //////////////////////
            player.SumOfRaiseTextBox.Select();
            //////////////////////
        }

        public void ConfirmRaise(object sender, EventArgs e)
        {
            int sum;
            try
            {
                sum = Convert.ToInt32(((GraphicPokerPlayer)ActivePlayer).SumOfRaiseTextBox.Text);
                if (IndexOfActivePlayerInPlayersList != 0)
                {
                    if (sum <= 0)
                    {
                        ShowMessage("The bet can not be smaller than 1");
                    }
                    else if (sum <= Players[IndexOfActivePlayerInPlayersList - 1].Bet)
                    {
                        ShowMessage("The sum of your bet must be bigger than the previous player's bet");
                    }
                    else if (sum > Players[IndexOfActivePlayerInPlayersList].Bank + Players[IndexOfActivePlayerInPlayersList].Bet)
                    {
                        ShowMessage("You can not bet more \"money\" than you have");
                    }
                    else
                    {
                        MakeInVisibleWriteSumOfRaiseMenu();
                        Raise(sum);
                    }
                }
                else
                {
                    if (sum <= 0)
                    {
                        ShowMessage("The raise can not be smaller than 1");
                    }
                    else if (sum <= Players[Players.Count - 1].Bet)
                    {
                        ShowMessage("The sum of the raise must be bigger than the previous player's bet");
                    }
                    else if (sum > Players[IndexOfActivePlayerInPlayersList].Bank + Players[IndexOfActivePlayerInPlayersList].Bet)
                    {
                        ShowMessage("You can not bet more \"money\" than you have");
                    }
                    else
                    {
                        MakeInVisibleWriteSumOfRaiseMenu();
                        Raise(sum);
                    }
                }
            }
            catch (Exception)
            {
                ShowMessage("Input format was uncorrect, please try again");
            }
        }

        public void Raise(int sum)
        {
            countOfPlayersThatChooseToCheck = 0;
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;
            List<Control> playersActionControls;

            if (player.CombinationValueLabel.Visible)
            {
                player.CombinationValueLabel.Visible = false;
            }
            if (player.CallButton.Visible == true)
            {
                playersActionControls = player.CallModeControls;
            }
            else
            {
                playersActionControls = player.CheckModeControls;
            }

            foreach (var control in playersActionControls)
            {
                control.Visible = false;
            }

            HideActivePlayersCards();

            player.Bet = sum;
            player.BetValueLabel.Text = player.Bet.ToString();
            player.BankValueLabel.Text = player.Bank.ToString();

            //////////////
            //if(player.Bet > BetOfTheRound) BetOfTheRound = player.Bet;
            BetOfTheRound = player.Bet;

            if (player.Bank == 0)
            {
                SomeoneDoAllInBet = true;
            }
            //////////////

            ChangeActivePlayer();
            player = (GraphicPokerPlayer)ActivePlayer;

            if (CheckIfCanActivePlayerCheck())
            {
                playersActionControls = player.CheckModeControls;
            }
            else
            {
                playersActionControls = player.CallModeControls;
            }

            foreach (var control in playersActionControls)
            {
                control.Visible = true;
            }
            if (CanTheActivePlayerMakeCombination)
            {
                ShowActivePlayerCombination();
            }
        }

        public override void Restart()
        {
            base.Restart();
            ((GraphicPokerPlayer)Winner).WinnerLabel.Visible = false;
            ((GraphicCardSet)CardsOnTable).Draw();

            //////////////////
            ((GraphicCardSet)Deck).Draw();
            //////////////////

            foreach (var player in Players)
            {
                GraphicPokerPlayer graphicPlayer = (GraphicPokerPlayer)player;
                graphicPlayer.CombinationValueLabel.Visible = false;
                graphicPlayer.BetValueLabel.Text = player.Bet.ToString();
                ((GraphicCardSet)player.HandCards).Draw();
            }

            /////////////////////////////////
            PauseBeetweenRounds_timer.Tick -= GoToLastRound_Tick;
            PauseBeetweenRounds_timer.Tick += PauseBeetweenRounds_timer_Tick;
            //////////////////////////////////
        }

        protected override void DeleteActivePlayerFromPlayersList()
        {
            base.DeleteActivePlayerFromPlayersList();
            BankValueLabel.Text = Bank.ToString();
            ((GraphicCardSet)ActivePlayer.HandCards).Draw();
        }

        protected override void DoSmallBlind()
        {
            ((GraphicPokerPlayer)ActivePlayer).Bet += SmallBlind;

            ////////////////////
            BetOfTheRound = ActivePlayer.Bet;
            ////////////////////
        }

        protected override void DoBigBlind()
        {
            ((GraphicPokerPlayer)ActivePlayer).Bet += SmallBlind * 2;

            ////////////////////
            BetOfTheRound = ActivePlayer.Bet;
            ////////////////////
        }

        public override void DealTwoCardsFromDeckToPlayer(string name)
        {
            PokerPlayer player = Players.Find(p => p.Name == name);
            base.DealTwoCardsFromDeckToPlayer(player);
            ((GraphicCardSet)player.HandCards).Draw();
        }

        /// <summary>
        /// Выводит победителя на экран
        /// </summary>
        protected void ShowWinner()
        {
            OpenCardsOfAllPlayersAndShowTheirCombinations();
            ((GraphicPokerPlayer)Winner).WinnerLabel.Visible = true;
        }

        /// <summary>
        /// Открывает карты всех игроков
        /// </summary>
        private void OpenCardsOfAllPlayersAndShowTheirCombinations()
        {
            foreach(var player in Players)
            {
                player.HandCards.Show();
                ((GraphicCardSet)player.HandCards).Draw();
                ((GraphicPokerPlayer)player).CombinationValueLabel.Visible = true;
            }
        }

        public void ShowActivePlayerCombination()
        {
            GraphicPokerPlayer player = (GraphicPokerPlayer)ActivePlayer;
            CardSet cardSet = new CardSet();
            cardSet.Add(CardsOnTable);
            cardSet.Add(player.HandCards);
            player.Combination = PokerCombination.GetCombination(cardSet);
            player.CombinationValueLabel.Text = player.Combination.ToString();
            player.CombinationValueLabel.Visible = true;
        }

        public void FoldWin()
        {
            IsGameOver = true;
            Winner = Players[0];
            SendGameBankToWinner();
            ShowWinner();
            ReturnFoldedPokerPlayersToPlayersList();
        }

        protected override void GameOver()
        {
            base.GameOver();
            //SendGameBankToWinner();
            ShowWinner();
        }

        protected override void SendGameBankToWinner()
        {
            base.SendGameBankToWinner();
            /*game*/BankValueLabel.Text = /*game*/Bank.ToString();
            ((GraphicPokerPlayer)Winner).BankValueLabel.Text = Winner.Bank.ToString();
        }

        private void ShowPlayersBanksInStartOfGame()
        {
            foreach (var player in Players)
            {
                ((GraphicPokerPlayer)player).BankValueLabel.Text = player.Bank.ToString();
            }
        }

        protected override void DoAutomaticBet()
        {
            if (!DidTheSmallBlindHasBeenDo)
            {
                ShowActivePlayerCards();
                //DoSmallBlind();
                DidTheSmallBlindHasBeenDo = true;
                DoAutomaticBets_timer.Start();
            }
            else if (!DidTheBigBlindHasBeenDo)
            {
                ShowActivePlayerCards();
                DoBigBlind();
                DidTheBigBlindHasBeenDo = true;
                HideActivePlayersCards();
                ChangeActivePlayer();
            }
            else
            {
                return;
            }
        }

        private void PauseBeetweenRounds_timer_Tick(object sender, EventArgs e)
        {
            if (countOfTimesPauseBeetweenRounds_timerWasCalled == 0)
            {
                bool IfSomeOneDoBet = IfSomeOneDoBetInThisRound();

                if (IfSomeOneDoBet)
                {
                    SendPlayersBetsToGameBank();
                }
                else
                {
                    DealCards();
                }
                countOfTimesPauseBeetweenRounds_timerWasCalled++;
            }
            else if (countOfTimesPauseBeetweenRounds_timerWasCalled == 1)
            {
                DealCards();
            }
            else
            {
                countOfTimesPauseBeetweenRounds_timerWasCalled = 0;
                PauseBeetweenRounds_timer.Stop();
                StartNewRound();
                CheckIfGameOver((GraphicPokerPlayer)ActivePlayer);
            }
        }

        private void DealCards()
        {
            DealCardsOnTable();
            countOfTimesPauseBeetweenRounds_timerWasCalled++;
        }

        private bool IfSomeOneDoBetInThisRound()
        {
            bool SomeoneDoBetInThisRound = false;
            foreach (var player in Players)
            {
                if (player.Bet != 0)
                {
                    SomeoneDoBetInThisRound = true;
                    break;
                }
            }

            return SomeoneDoBetInThisRound;
        }

        protected override void DealCardsOnTable()
        {
            base.DealCardsOnTable();
            ((GraphicCardSet)CardsOnTable).Draw();
        }

        protected override void SendPlayersBetsToGameBank()
        {
            base.SendPlayersBetsToGameBank();
            foreach (var player in Players)
            {
                ((GraphicPokerPlayer)player).BetValueLabel.Text = player.Bet.ToString();
            }
            /*game*/
            bankValueLabel.Text = Bank.ToString();
        }

        private void DealCardsToPlayers_timer_Tick(object sender, EventArgs e)
        {
            if (countOfTimesDealCardsToPlayers_timerWasCalled < Players.Count)
            {
                DealTwoCardsFromDeckToPlayer(Players[countOfTimesDealCardsToPlayers_timerWasCalled].Name);
                countOfTimesDealCardsToPlayers_timerWasCalled++;

                if (countOfTimesDealCardsToPlayers_timerWasCalled == Players.Count)
                {
                    DealCardsToPlayers_timer.Stop();
                    countOfTimesDealCardsToPlayers_timerWasCalled = 0;
                    DoAutomaticBets_timer.Start();
                }
            }
        }

        private void DoAutomaticBets_timer_Tick(object sender, EventArgs e)
        {
            if (countOfTimesDoAutomaticBets_timerWasCalled < 3)
            {
                if (countOfTimesDoAutomaticBets_timerWasCalled == 0)
                {
                    DoAutomaticBets_timer.Stop();
                    countOfTimesDoAutomaticBets_timerWasCalled++;
                    DoAutomaticBet();
                }
                else if (countOfTimesDoAutomaticBets_timerWasCalled == 1)
                {
                    countOfTimesDoAutomaticBets_timerWasCalled++;
                    HideActivePlayersCards();
                    //////////
                    DoSmallBlind();
                    ///////////
                    ChangeActivePlayer();
                }
                else
                {
                    DoAutomaticBets_timer.Stop();
                    countOfTimesDoAutomaticBets_timerWasCalled = 0;
                    DoAutomaticBet();
                }
            }
        }

        public void DealCardsToPlayers()
        {
            DealCardsToPlayers_timer.Start();
        }
    }
}
