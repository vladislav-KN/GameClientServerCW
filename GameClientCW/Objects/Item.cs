
using System;
using System.Collections.Generic;

namespace GameClass.Objects
{
    [Serializable]
    public class Item : GameObj
    {
        protected string name, discription;
        protected List<Params> parametrs;
        protected int cost;
        protected int id;

        public string Name 
        { 
            get 
            {
                return name;
            } 
        }
        public string Discription
        {
            get
            {
                return discription;
            }
        }
        public int ID {
            get
            {
                return id;
            }
        }
        public int Cost
        {
            get
            {
                return cost;
            }
        }
        public List<Params> Parametrs
        {
            get
            {
                return parametrs;
            }
        }

        public Item(string ItemName, string ItemDeisc, List<Params> ItemParam, int ItemCost)
        {
            name = ItemName;
            discription = ItemDeisc;
            parametrs = ItemParam;
            cost = ItemCost;
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
    public class AdminItemTools : Item, Administriting
    {
        public AdminItemTools(string ItemName, string ItemDeisc, List<Params> ItemParam, int ItemCost) : base(ItemName, ItemDeisc, ItemParam, ItemCost)
        {
        }

        public string Name
        {
            set
            {
                name = value;
                Update();
            }
        }
        public string Discription
        {
            set
            {
                discription = value;
                Update();
            }
        }
        public int Cost
        {
            set
            {
                Cost = value;
                Update();
            }
        }
        public List<Params> Parametrs
        {
            set
            {
                Parametrs = value;
                Update();
            }
        }

        public void Add(GameObj gameObj)
        {   
            if(name!= null && discription != null && parametrs != null && cost != default)
            {

            }
             
        }

        public void Change(GameObj gameObj)
        {

        }

        public void Delete(GameObj gameObj)
        {

        }
    }
}