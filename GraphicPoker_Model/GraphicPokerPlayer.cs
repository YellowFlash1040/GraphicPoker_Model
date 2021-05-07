using System.Collections.Generic;
using System.Windows.Forms;
using GraphicCards;
using Poker_Model;

namespace GraphicPoker_Model
{
    public class GraphicPokerPlayer : PokerPlayer
    {
        public GraphicPokerPlayer(string name, GraphicCardSet handCards, int bank, Label bankValueLabel, Label betValueLabel,
            Label raiseButton, Label checkButton, Label foldButton, Label callButton, Button confirmRaiseButton,
            Button cancelRaiseButton, TextBox sumOfRaiseValueTextBox, Label winnerLabel, Label combinationValueLabel) : base(name, handCards, bank)
        {
            this.raiseButton = raiseButton;
            this.foldButton = foldButton;
            this.checkButton = checkButton;
            this.callButton = callButton;
            this.confirmRaiseButton = confirmRaiseButton;
            this.cancelRaiseButton = cancelRaiseButton;
            this.sumOfRaiseTextBox = sumOfRaiseValueTextBox;
            this.betValueLabel = betValueLabel;
            this.bankValueLabel = bankValueLabel;
            this.winnerLabel = winnerLabel;
            this.combinationValueLabel = combinationValueLabel;

            checkModeControls = new List<Control>();
            checkModeControls.Add(this.foldButton);
            checkModeControls.Add(this.checkButton);
            checkModeControls.Add(this.raiseButton);

            callModeControls = new List<Control>();
            callModeControls.Add(this.foldButton);
            callModeControls.Add(this.callButton);
            callModeControls.Add(this.raiseButton);

            raiseButttonMenuControls = new List<Control>();
            raiseButttonMenuControls.Add(this.sumOfRaiseTextBox);
            raiseButttonMenuControls.Add(this.confirmRaiseButton);
            raiseButttonMenuControls.Add(this.cancelRaiseButton);
        }

        public override int Bank 
        {
            get => base.Bank;
            set
            {
                base.Bank = value;
                if(this.BankValueLabel != null)
                {
                    this.BankValueLabel.Text = Bank.ToString();
                }
            }
        }

        public override int Bet 
        {
            get => base.Bet;
            set
            {
                base.Bet = value;
                if (this.BetValueLabel != null)
                {
                    this.BetValueLabel.Text = Bet.ToString();
                }
            }
        }

        private Label combinationValueLabel;
        public Label CombinationValueLabel
        {
            get
            {
                return combinationValueLabel;
            }
        }

        private Label winnerLabel;
        public Label WinnerLabel
        {
            get
            {
                return winnerLabel;
            }
        }

        private List<Control> raiseButttonMenuControls;
        public List<Control> RaiseButttonMenuControls
        {
            get
            {
                return raiseButttonMenuControls;
            }
        }

        private List<Control> callModeControls;
        public List<Control> CallModeControls
        {
            get
            {
                return callModeControls;
            }
        }

        private List<Control> checkModeControls;
        public List<Control> CheckModeControls
        {
            get
            {
                return checkModeControls;
            }
        }


        private TextBox sumOfRaiseTextBox;
        public TextBox SumOfRaiseTextBox
        {
            get
            {
                return sumOfRaiseTextBox;
            }
        }

        private Button confirmRaiseButton;
        public Button ConfirmRaiseButton
        {
            get
            {
                return confirmRaiseButton;
            }
        }

        private Button cancelRaiseButton;
        public Button CancelRaiseButton
        {
            get
            {
                return cancelRaiseButton;
            }
        }

        private Label raiseButton;
        public Label RaiseButton
        {
            get
            {
                return raiseButton;
            }
        }

        private Label foldButton;
        public Label FoldButton
        {
            get
            {
                return foldButton;
            }
        }

        private Label callButton;
        public Label CallButton
        {
            get
            {
                return callButton;
            }
        }

        private Label checkButton;
        public Label CheckButton
        {
            get
            {
                return checkButton;
            }
        }

        private Label bankValueLabel;
        public Label BankValueLabel
        {
            get
            {
                return bankValueLabel;
            }
        }

        private Label betValueLabel;
        public Label BetValueLabel
        {
            get
            {
                return betValueLabel;
            }
        }
    }
}
