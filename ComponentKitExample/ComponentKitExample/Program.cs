using ComponentKit;
using ComponentKit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComponentKitExample
{
    class Health : Component
    {
        public Health()
        {
            Amount = 100;
        }

        public bool IsDead
        {
            get
            {
                return Amount <= 0;
            }
        }

        public int Amount
        {
            get;
            set;
        }
    }

    class Combustible : DependencyComponent
    {
        [RequireComponent]
        Health _health;

        public bool IsBurning
        {
            get;
            private set;
        }

        public void Combust()
        {
            IsBurning = true;
        }

        public void Extinguish()
        {
            IsBurning = false;
        }

        public void Burn()
        {
            Console.WriteLine("Burning :( ");
            _health.Amount -= 10;
        }
    }

    class FireBreathing : DependencyComponent
    {
        public void Breathe(IEntityRecord target)
        {
            if (target.HasComponent<Combustible>())
            {
                Console.WriteLine("Firebreathe !!!");
                target.GetComponent<Combustible>().Combust();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            Entity.Define("Dragon", typeof(Health), typeof(FireBreathing));

            Entity.Define("Player",
                typeof(Health),
                typeof(Combustible));

            IEntityRecord player = Entity.CreateFromDefinition("Player", "You!");

            IEntityRecord dragon = Entity.CreateFromDefinition("Dragon", "A firebreathing dragon!");

            List<Combustible> combustibles = new List<Combustible>()
            {
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
                new Combustible(),
            };

            foreach (Combustible item in combustibles)
            {
                player.Add(item);
            }


            EntityRegistry.Current.SetTrigger(
                component => component is Combustible,
                (sender, eventArgs) =>
                {
                    foreach (IComponent component in eventArgs.Components)
                    {
                        Combustible combustible = component as Combustible;

                        if (combustible != null)
                        {
                            if (combustible.IsOutOfSync)
                            {
                                combustibles.Remove(combustible);
                            }
                            else
                            {
                                combustibles.Add(combustible);
                            }
                        }
                    }
                });

            EntityRegistry.Current.Synchronize();

            dragon.GetComponent<FireBreathing>().Breathe(player);

            Random r = new Random();

            for (int turn = 0; turn < 10; turn++)
            {
                foreach (Combustible combustible in combustibles)
                {
                    if (combustible.IsBurning)
                    {
                        combustible.Burn();
                    }
                }

                Combustible fire = player.GetComponent<Combustible>();
                Health condition = player.GetComponent<Health>();

                if (fire.IsBurning)
                {
                    // There's a 10% chance that the player figures out how to extinguish himself!
                    bool playerStoppedDroppedAndRolled =
                        r.Next(0, 100) <= 10;

                    if (playerStoppedDroppedAndRolled)
                    {
                        Console.WriteLine("Player extinguished himself !!!");
                        fire.Extinguish();
                    }
                }

                if (condition.IsDead)
                {
                    // Unfortunately for the player, he did not figure it out in time.
                    Console.WriteLine("Player is dead ://");
                    break;
                }
            }
        }
    }
}
