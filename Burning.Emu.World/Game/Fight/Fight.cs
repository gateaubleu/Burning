﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Burning.DofusProtocol.Enums;
using Burning.DofusProtocol.Network.Messages;
using Burning.DofusProtocol.Network.Types;
using Burning.Emu.World.Entity;
using Burning.Emu.World.Game.Fight.Fighters;
using Burning.Emu.World.Game.Map;
using Burning.Emu.World.Game.PathFinder;
using Burning.Emu.World.Game.World;
using Burning.Emu.World.Network;
using FlatyBot.Common.Network;

namespace Burning.Emu.World.Game.Fight
{
    public class Fight
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public Fighter ActualFighter { get; set; }

        public List<Fighter> Defenders { get; set; }

        public List<Fighter> Challengers { get; set; }

        public FightTypeEnum FightType { get; set; }

        public FightStartingPositions FightStartingPositions { get; set; }

        public FightStateEnum FightState { get; set; }

        public List<WorldClient> clientsInFight { get; set; }

        public int Round { get; set; }

        private Timer TurnTimer { get; set; }

        private Timer PlacementPhaseTimer { get; set; }

        public Fight(int mapId, FightTypeEnum type, List<Fighter> defenders, List<Fighter> challengers, FightStartingPositions fightStartingPositions)
        {
            this.Id = 1; //Uniqid a faire
            this.MapId = mapId;
            this.FightType = type;
            this.Defenders = defenders;
            this.Challengers = challengers;
            this.FightStartingPositions = fightStartingPositions;
            this.FightState = FightStateEnum.FIGHT_CHOICE_PLACEMENT;
            this.Round = 0;

            this.StartPlacementPhaseTimer();
        }

        public void EnterFight(WorldClient client)
        {
            foreach (var fighter in this.Challengers.Concat(this.Defenders))
            {
                IdentifiedEntityDispositionInformations identifiedEntityDispositionInformations = new IdentifiedEntityDispositionInformations((int)fighter.CellId, 1, (double)fighter.Id);
                client.SendPacket(new GameEntitiesDispositionMessage(new List<IdentifiedEntityDispositionInformations>() { identifiedEntityDispositionInformations }));

                if (fighter is CharacterFighter)
                    client.SendPacket(new GameFightShowFighterMessage(((CharacterFighter)fighter).GetGameFightCharacterInformations()));
                else if (fighter is MonsterFighter)
                    client.SendPacket(new GameFightShowFighterMessage(((MonsterFighter)fighter).GetGameFightMonsterInformations()));
            }
        }

        public bool CanChangeStartingPositions(WorldClient client, int requestedCellId)
        {

            if (this.FightState != FightStateEnum.FIGHT_CHOICE_PLACEMENT)
                return false;

            bool isDefender = this.Defenders.Find(x => x is CharacterFighter && x.Id == client.ActiveCharacter.Id) != null ? true : false;

            if(isDefender)
            {
                bool isStartingPos = this.FightStartingPositions.positionsForDefenders.Find(x => x == requestedCellId) != 0 ? true : false;
                if (!isStartingPos)
                    return false;

                bool isFreeCell = this.Defenders.Find(x => x.CellId == requestedCellId) != null ? true : false;
                if (isFreeCell)
                    return false;

                //update cellid
                this.Defenders.Find(x => x is CharacterFighter && x.Id == client.ActiveCharacter.Id).CellId = requestedCellId;
            }
            else
            {
                bool isStartingPos = this.FightStartingPositions.positionsForChallengers.Find(x => x == requestedCellId) != 0 ? true : false;
                if (!isStartingPos)
                    return false;

                bool isFreeCell = this.Challengers.Find(x => x.CellId == requestedCellId) != null ? true : false;
                if (isFreeCell)
                    return false;

                //update cellid
                this.Challengers.Find(x => x is CharacterFighter && x.Id == client.ActiveCharacter.Id).CellId = requestedCellId;
            }

            List<IdentifiedEntityDispositionInformations> dispositions = new List<IdentifiedEntityDispositionInformations>();
            foreach (var fighter in this.Challengers.Concat(this.Defenders))
            {
                IdentifiedEntityDispositionInformations identifiedEntityDispositionInformations = new IdentifiedEntityDispositionInformations((int)fighter.CellId, 1, fighter.Id);
                dispositions.Add(identifiedEntityDispositionInformations);
            }

            //update position
            client.SendPacket(new GameEntitiesDispositionMessage(dispositions));

            return true;
        }

        private void SendToAllFighters(List<NetworkMessage> messages)
        {
            foreach (var fighter in this.Challengers.Concat(this.Defenders).ToList().FindAll(x => x is CharacterFighter))
            {
                var client = WorldManager.Instance.GetClientFromCharacter(((CharacterFighter)fighter).Character);
                if (client != null)
                {
                    foreach (var msg in messages)
                    {
                        client.SendPacket(msg);
                    }
                }
            }

        }

        public void TurnEnd()
        {
            //queue message
            List<NetworkMessage> messages = new List<NetworkMessage>();
            messages.Add(new GameFightTurnEndMessage((double)this.ActualFighter.Id)); //fin du tour
            messages.Add(new GameFightTurnReadyRequestMessage((double)this.ActualFighter.Id));


            //reset AP/PM
            if (this.ActualFighter is CharacterFighter)
                ((CharacterFighter)this.ActualFighter).ResetFighter();

            TurnTimer.Stop();

            var aliveFighters = this.Challengers.Concat(this.Defenders).OrderBy(x => x.TimelineOrder).ToList().FindAll(x => x.Life > 0);
            var nextFighter = this.Challengers.Concat(this.Defenders).OrderBy(x => x.TimelineOrder).ToList().Find(x => x.TimelineOrder > this.ActualFighter.TimelineOrder && x.Life > 0);

            if (nextFighter == null)
            {
                if (aliveFighters.Count > 1)
                {
                    this.ActualFighter = this.Challengers.Concat(this.Defenders).OrderBy(x => x.TimelineOrder).ToList().First();
                    this.Round += 1;
                    messages.Add(new GameFightNewRoundMessage((uint)this.Round));
                }
                else
                {
                    Console.WriteLine("Fin du combat !");
                    return;
                }
            }
            else
            {
                this.ActualFighter = nextFighter;
            }


            Console.WriteLine("FIN DU TOUR DE JEU");
            Console.WriteLine("NOUVEAU TOUR POUR {0} authorId.", this.ActualFighter.Id);

            int nextTurnSecondes = 320;
            if (this.ActualFighter is MonsterFighter)
                nextTurnSecondes = 50;

            //calcul temps additionnel = time restant / 2 entier le plus bas

            messages.Add(new GameFightTurnStartMessage(this.ActualFighter.Id, (uint)nextTurnSecondes)); //nouveau tour

            this.SendToAllFighters(messages);

            this.StartTurnTimer(nextTurnSecondes);
        }

        public void MovementRequestSequence(int requestedCellId)
        {
            var usedCells = this.Defenders.Concat(this.Challengers).Where(f => f.Life > 0).Select(x => (int)x.CellId).ToArray();
            var map = MapManager.Instance.GetMap(this.MapId);

            var path = new Pathfinder(usedCells);
            path.SetMap(map.MapData, false);

            var cells = path.GetPath((short)this.ActualFighter.CellId, (short)requestedCellId).Select(x => (uint)x.Id).ToList();

            if (cells.Count <= 1)
                return;

            var cellDistance = (cells.Count - 1); // taille du déplacement - la cell de départ

            if (cellDistance > this.ActualFighter.PM)
                return;

            this.ActualFighter.CellId = MapManager.Instance.GetCellIdFromKeyMovement((int)cells[cells.Count - 1]);
            this.ActualFighter.PM -= cellDistance; //- distance

            List<NetworkMessage> queueMessages = new List<NetworkMessage>();
            queueMessages.Add(new SequenceStartMessage((int)SequenceTypeEnum.SEQUENCE_MOVE, this.ActualFighter.Id));
            queueMessages.Add(new GameMapMovementMessage(cells, 3, this.ActualFighter.Id));
            queueMessages.Add(new GameActionFightPointsVariationMessage(129, this.ActualFighter.Id, this.ActualFighter.Id, -(cellDistance)));
            queueMessages.Add(new SequenceEndMessage(3, this.ActualFighter.Id, (int)SequenceTypeEnum.SEQUENCE_MOVE));

            this.SendToAllFighters(queueMessages);
        }

        public void StartPlacementPhaseTimer()
        {
            if (this.FightState != FightStateEnum.FIGHT_CHOICE_PLACEMENT)
                return;

            PlacementPhaseTimer = new Timer(5000);
            PlacementPhaseTimer.Elapsed += Timer_Elapsed;
            PlacementPhaseTimer.Enabled = true;
        }

        public void StartTurnTimer(int millisecondes)
        {
            if (this.FightState != FightStateEnum.FIGHT_STARTED)
                return;

            TurnTimer = new Timer(millisecondes * 100);
            TurnTimer.Elapsed += Timer_Elapsed;
            TurnTimer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            switch(this.FightState)
            {
                case FightStateEnum.FIGHT_CHOICE_PLACEMENT:
                    PlacementPhaseTimer.Stop();

                    //actualise l'ordre dans la timeline
                    var orderedFighters = this.Challengers.Concat(this.Defenders).OrderByDescending(x => x.Initiative).ToList();
                    for(int i = 0; i < orderedFighters.Count; i++)
                    {
                        orderedFighters[i].TimelineOrder = (i + 1);
                    }


                    List<NetworkMessage> messages = new List<NetworkMessage>();
                    messages.Add(new GameFightStartMessage(new List<Idol>()));
                    messages.Add(new GameFightTurnListMessage(orderedFighters.Select(x => (double)x.Id).ToList(), new List<double> { }));
                    
                    this.ActualFighter = orderedFighters[0];
                    this.Round = 1;
                    this.FightState = FightStateEnum.FIGHT_STARTED;


                    messages.Add(new GameFightNewRoundMessage((uint)this.Round));
                    messages.Add(new GameFightTurnStartMessage(this.ActualFighter.Id, 320));

                    this.SendToAllFighters(messages);

                    this.StartTurnTimer(50);
                    break;
                case FightStateEnum.FIGHT_STARTED:
                    //todo faire une fonction qui turnEnd
                    this.TurnEnd();
                    break;
            }
        }
    }
}
