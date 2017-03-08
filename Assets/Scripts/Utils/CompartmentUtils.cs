using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Loaders;
using Newtonsoft.Json;

public enum GroupType
{
    Proteins = 0,
    Lipids = 1,
    Fibers = 2
};

namespace Assets.Scripts.Loaders
{
    public class CompartmentUtility
    {
        public const string PATH_SEPARATOR = ".";

        public static Compartment DeserializeJson(string path)
        {
            var str = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Compartment>(str);
        }

        public static void PostProcessSceneGraph(Compartment rootCompartment)
        {
            var compartments = new List<Compartment>();
            var queue = new Queue<Compartment>();

            rootCompartment.parent_id = -1;
            rootCompartment.path = "root";

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();
                compartments.Add(currentCompartment);

                foreach (var ingredientGroup in currentCompartment.IngredientGroups)
                {
                    ingredientGroup.compartment_id = currentCompartment.unique_id;
                    ingredientGroup.path = currentCompartment.path + PATH_SEPARATOR + ingredientGroup.name;

                    foreach (var ingredient in ingredientGroup.Ingredients)
                    {
                        ingredient.name = ingredient.name.Split(new[] {"__"}, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
                        ingredient.ingredient_group_id = ingredientGroup.unique_id;
                        ingredient.path = ingredientGroup.path + PATH_SEPARATOR + ingredient.name;
                    }
                }

                foreach (var compartment in currentCompartment.Compartments)
                {
                    compartment.parent_id = currentCompartment.unique_id;
                    compartment.path = currentCompartment.path + PATH_SEPARATOR + compartment.name;
                    queue.Enqueue(compartment);
                }
            }
        }

        public static List<Compartment> GetCompartments(Compartment rootCompartment)
        {
            var compartments = new List<Compartment>();
            var queue = new Queue<Compartment>();

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();
                compartments.Add(currentCompartment);

                foreach (var compartment in currentCompartment.Compartments)
                {
                    queue.Enqueue(compartment);
                }
            }

            return compartments;
        }

        public static List<IngredientGroup> GetIngredientGroups(Compartment rootCompartment)
        {
            var groups = new List<IngredientGroup>();
            var queue = new Queue<Compartment>();

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();
                groups.AddRange(currentCompartment.IngredientGroups);

                foreach (var compartment in currentCompartment.Compartments)
                {
                    queue.Enqueue(compartment);
                }
            }

            return groups;
        }

        //*****************************

        public static List<Ingredient> GetProteinIngredients(List<IngredientGroup> ingredientGroups)
        {
            var ingredients = new List<Ingredient>();

            foreach (var ingredientGroup in ingredientGroups)
            {
                //if (ingredientGroup.group_type == (int)IngredientGroupType.INTERIOR_PROTEINS || ingredientGroup.group_type == (int)IngredientGroupType.SURFACE_PROTEINS)
                if (true)
                {
                    foreach (var ingredient in ingredientGroup.Ingredients)
                    {
                        ingredient.ingredient_group_id = ingredientGroup.unique_id;
                        ingredient.path = ingredientGroup.path + PATH_SEPARATOR + ingredient.name;
                        ingredients.Add(ingredient);
                    }
                }
            }

            return ingredients;
        }

        public static List<Ingredient> GetProteinIngredients(Compartment rootCompartment)
        {
            var ingredients = new List<Ingredient>();
            var queue = new Queue<Compartment>();

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();

                foreach (var ingredientGroup in currentCompartment.IngredientGroups)
                {
                    //if (ingredientGroup.group_type == (int)IngredientGroupType.INTERIOR_PROTEINS || ingredientGroup.group_type == (int)IngredientGroupType.SURFACE_PROTEINS)
                    if (true)
                    {
                        ingredients.AddRange(ingredientGroup.Ingredients);
                    }
                }

                foreach (var compartment in currentCompartment.Compartments)
                {
                    queue.Enqueue(compartment);
                }
            }

            return ingredients;
        }


        //public static List<IngredientGroup> GetAllIngredientGroups(List<Compartment> Compartments)
        //{
        //    var ingredientGroups = new List<IngredientGroup>();

        //    foreach (var compartment in Compartments)
        //    {
        //        foreach (var ingredientGroup in compartment.IngredientGroups)
        //        {
        //            ingredientGroup.compartment_id = compartment.unique_id;
        //            ingredientGroup.path = compartment.path + PATH_SEPARATOR + ingredientGroup.name;
        //            ingredientGroups.Add(ingredientGroup);
        //        }
        //    }

        //    return ingredientGroups;
        //}

        //public static List<Ingredient> GetAllProteinIngredients(List<IngredientGroup> ingredientGroups)
        //{
        //    var ingredients = new List<Ingredient>();

        //    foreach (var ingredientGroup in ingredientGroups)
        //    {
        //        if (ingredientGroup.group_type == (int)IngredientGroupType.INTERIOR_PROTEINS
        //            || ingredientGroup.group_type == (int)IngredientGroupType.SURFACE_PROTEINS)
        //        {
        //            foreach (var ingredient in ingredientGroup.Ingredients)
        //            {
        //                ingredient.ingredient_group_id = ingredientGroup.unique_id;
        //                ingredient.path = ingredientGroup.path + PATH_SEPARATOR + ingredient.name;
        //                ingredients.Add(ingredient);
        //            }
        //        }
        //    }

        //    return ingredients;
        //}
    }

    public enum IngredientType { PROTEIN = 0, LIPID, FIBER };
    public enum IngredientGroupType { INTERIOR_PROTEINS = 0, SURFACE_PROTEINS, LIPIDS, FIBERS };

    [Serializable]
    public class Compartment
    {
        public string _name;
        public string _path;
        public int _localId;
        public int _uniqueId;
        public int _parentId;

        [NonSerialized]
        private List<Compartment> _compartments;
        [NonSerialized]
        private List<IngredientGroup> _ingredientGroups;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string path
        {
            get { return _path; }
            set { _path = value; }
        }

        public int local_id
        {
            get { return _localId; }
            set { _localId = value; }
        }

        public int unique_id
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public int parent_id
        {
            get { return _parentId; }
            set { _parentId = value; }
        }
        
        public List<Compartment> Compartments
        {
            get { return _compartments; }
            set { _compartments = value; }
        }

        public List<IngredientGroup> IngredientGroups
        {
            get { return _ingredientGroups; }
            set { _ingredientGroups = value; }
        }
    }

    [Serializable]
    public class IngredientGroup
    {
        public int NumIngredients;

        public bool _fiber;
        public string _name;
        public string _path;
        public int _localId;
        public int _uniqueId;
        public int _groupType;
        public int _compartmentId;
        public List<Ingredient> _ingredients;

        public bool fiber
        {
            get { return _fiber; }
            set { _fiber = value; }
        }

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string path
        {
            get { return _path; }
            set { _path = value; }
        }

        public int local_id
        {
            get { return _localId; }
            set { _localId = value; }
        }

        public int unique_id
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public int group_type
        {
            get { return _groupType; }
            set { _groupType = value; }
        }

        public int compartment_id
        {
            get { return _compartmentId; }
            set { _compartmentId = value; }
        }

        public List<Ingredient> Ingredients
        {
            get { return _ingredients; }
            set { _ingredients = value; }
        }
    }

    [Serializable]
    public class Ingredient
    {
        public string _name;
        public string _path;

        public int _nbMol;
        public int _nbChains;
        public int _ingredientId;
        public int _ingredientGroupId;

        public Source _source;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string path
        {
            get { return _path; }
            set { _path = value; }
        }

        public int ingredient_id
        {
            get { return _ingredientId; }
            set { _ingredientId = value; }
        }
        
        public int ingredient_group_id
        {
            get { return _ingredientGroupId; }
            set { _ingredientGroupId = value; }
        }

        public int nbMol
        {
            get { return _nbMol; }
            set { _nbMol = value; }
        }

        public int nbChains
        {
            get { return _nbChains; }
            set { _nbChains = value; }
        }

       

        public Source source
        {
            get { return _source; }
            set { _source = value; }
        }
    }

    [Serializable]
    public class Source
    {
        public bool _biomt;
        public string _pdb;
        public Transform _transform;

        public Source()
        {
            pdb = "";
        }

        public bool biomt
        {
            get { return _biomt; }
            set { _biomt = value; }
        }

        public string pdb
        {
            get { return _pdb; }
            set { _pdb = value; }
        }

        public Transform transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
    }

    [Serializable]
    public class Transform
    {
        public bool _center;

        public bool center
        {
            get { return _center; }
            set { _center = value; }
        }

        //public List<double> rotate { get; set; }
        //public List<double> translate { get; set; }
    }
}
