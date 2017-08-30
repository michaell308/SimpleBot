using Discord;
using Discord.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBot
{
    class MyBot
    {
        DiscordClient discord;
        public MyBot()
        {

            var rnd = new Random();

            discord = new DiscordClient();

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            // -- COMMANDS --

            var commands = discord.GetService<CommandService>();

            // HELLO : greets user in the same language
            String[] engHiArr = new String[] { "Hi", "Hello", "Hey", "Howdy", "Yo", "Sup", "Hiya", "'ello", "Greetings" };
            String[] japHiArr = new String[] { "Konnichiwa", "Ohayō gozaimasu", "Ohayo gozaimasu", "Ohayō", "Ohayo" };
            String[] polHiArr = new String[] { "Cześć", "Czesc", "Dzień dobry", "Dzien dobry" };
            commands.CreateCommand("Hi")
                .Alias(engHiArr.Concat(japHiArr).Concat(polHiArr).ToArray())
                .Do(async (e) =>
                {
                    var greeting = "Hi";
                    //find language bot was greeted in, and choose random greeting from matching language
                    if (engHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = engHiArr.ElementAt((rnd.Next(0, engHiArr.Length)));
                    }
                    else if (japHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = japHiArr.ElementAt((rnd.Next(0, japHiArr.Length)));
                    }
                    else if (polHiArr.Contains(e.Message.Text.Substring(1), StringComparer.OrdinalIgnoreCase))
                    {
                        greeting = polHiArr.ElementAt((rnd.Next(0, polHiArr.Length)));
                    }

                    await e.Channel.SendMessage(greeting + " " + e.User.Name);
                });

            // RANDUSER : choose random user; if given message, display message after user (i.e. randomUser "won a prize!") 
            commands.CreateCommand("randUser")
                .Alias("ru", "randomuser", "ruser")
                .Parameter("message", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var userList = e.Channel.Users;
                    User randomUser = userList.ElementAt((rnd.Next(0, userList.Count())));

                    if (e.GetArg("message") == "") //default, no message given
                    {
                        await e.Channel.SendMessage(e.User.Name + " randomly chose... " + randomUser.Mention);
                    }
                    else //give user and message
                    {
                        await e.Channel.SendMessage(randomUser.Mention + " " + String.Join(String.Empty, e.Args.ToArray()));
                    }
                });

            // FLIPCOIN : flip a coin and show result (heads or tails)
            commands.CreateCommand("flipCoin")
            .Alias("fc", "flip", "coin", "coinflip", "cf")
            .Do(async (e) =>
            {
                int curNum = (new Random().Next(0, 2)); //choose either 0 or 1
                String flippedCoin = "Heads";
                if (curNum == 1)
                {
                    flippedCoin = "Tails";
                }
                await e.Channel.SendMessage(e.User.Name + " flipped a coin: " + flippedCoin);
            });

            // ROLL : roll dice; supports rolling any sided die and any number of dice, along with ability to combine with numbers
            //ex: !r d4 (roll one four-sided dice), !r 2d10 (roll two d10), !r 15d2 + 3 (roll fifteen d2 and add 3 to the result)
            //combines input left to right, i.e. 2 + 3 * 4 is ((2 + 3) * 4)
            commands.CreateCommand("roll")
                .Alias("r", "dice")
                .Parameter("dice", ParameterType.Multiple)
                .Do(async (e) =>
                {
                    int total = 0;
                    if (validStmt(e.Args, ref total))
                    {
                        await e.Channel.SendMessage(e.User.Mention + " rolled " + total);
                    }
                    else
                    {
                        await e.Channel.SendMessage("Sorry " + e.User.Mention + "! I didn't understand your roll.");
                    }
                });

            //returns true if given args make up a valid roll statement
            //puts roll result into total
            bool validStmt(string[] args, ref int total)
            {
                char prevOp = '+';
                if (args.Length % 2 != 0) //need odd number of arguments (!r "d2" "+" "5", not !r "2d6" "+")
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        String curArg = args.ElementAt(i);
                        if (i % 2 == 0) //even arguments should be dice
                        {
                            if (validDice(curArg))
                            {
                                int rRes = rollResult(curArg);
                                if (rRes != -1) {
                                    total = execOp(total, rRes, prevOp);
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else //odd arguments should be operators
                        {
                            if (validOperator(curArg))
                            {
                                prevOp = curArg[0];
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }

            //returns true if given string is a valid dice argument
            bool validDice(String diceStr)
            {
                if (diceStr.All(Char.IsDigit)) //just a number ("5" or "100")
                {
                    return true;
                }
                int dIndex = diceStr.IndexOf('d'); //find first 'd' char, if there is one
                if (dIndex != -1 && dIndex != diceStr.Length - 1) //diceStr contains 'd' and that 'd' isn't the last char
                {
                    var diceStrNoD = diceStr.Where((v, i) => i != dIndex).ToList(); //remove that 'd' from diceStr
                    if (diceStrNoD.Count >= 1 && diceStrNoD.All(Char.IsDigit)) //there are remaining chars, and all of them are digits
                    {
                        return true;
                    }
                }
                return false;
            }

            //returns the result of rolling the dice specified in diceStr
            int rollResult(String diceStr)
            {
                int x = 0; //used for TryParse
                int numDice = 1;
                int dIndex = diceStr.IndexOf('d'); //find first 'd' char
                if (diceStr.All(Char.IsDigit)) //just a number ("5" or "100")
                {
                    return Int32.TryParse(diceStr, out x) ? x : -1;
                }
                else if (diceStr[0] != 'd') //multiple dice i.e. "2d6"
                {
                    String numDiceStr = diceStr.Substring(0, dIndex);
                    numDice = Int32.TryParse(numDiceStr, out x) ? x : -1;
                    if (numDice == -1) //parse failed
                    {
                        return -1;
                    }
                }
                //roll typeDice (d6,d8,etc) numDice times, store in retVal
                int retVal = 0;
                String typeDiceStr = diceStr.Substring(dIndex + 1);
                int typeDice = Int32.TryParse(typeDiceStr, out x) ? x : -1;
                if (typeDice == -1) //parse failed
                {
                    return -1;
                }
                for (int j = 0; j < numDice; j++)
                {
                    try
                    {
                        retVal = checked (retVal + rollDice(typeDice));
                    }
                    catch (OverflowException) //retVal too large, just return maxValue int
                    {
                        return int.MaxValue;
                    }
                }
                return retVal;
            }

            //roll a die, choosing a number between 1 and maxVal (including)
            int rollDice(int maxVal)
            {
                return (rnd.Next(1, maxVal + 1));
            }

            //returns true if opStr is a valid operator
            bool validOperator(String opStr)
            {
                return opStr.Length == 1 && opStr.IndexOfAny("+-*/%^".ToCharArray()) != -1;
            }

            //return combination of num1 and num2 using given operation
            //if no operation matches, returns -1
            int execOp(int num1, int num2, char op)
            {
                if (op == '+')
                {
                    return num1 + num2;
                }
                else if (op == '-')
                {
                    return num1 - num2;
                }
                else if (op == '*')
                {
                    return num1 * num2;
                }
                else if (op == '/')
                {
                    return num1 / num2;
                }
                else if (op == '%')
                {
                    return num1 % num2;
                }
                else if (op == '^')
                {
                    return (int)Math.Pow(num1, num2);
                }
                return -1;
            }

            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect({bot-token}, TokenType.Bot);
            });
        }

        
    }
}
