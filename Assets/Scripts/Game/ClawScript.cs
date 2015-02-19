using UnityEngine;
using System.Collections;

public class ClawScript : MonoBehaviour {

    private IComparer rayHitComparer = new RayHitComparer();

	public void Attack()
    {
        Ray ray = new Ray(transform.parent.parent.position, transform.parent.parent.forward);

        RaycastHit[] hits = Physics.RaycastAll(ray, 1f);
        System.Array.Sort(hits, rayHitComparer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag == "Player")
            {
                hits[i].collider.gameObject.GetComponent<Vitals>().TakeDamage(10, "Funyarinpa");
            }
        }
    }


    /// <summary>
    /// Used for comparing distances
    /// </summary>
    public class RayHitComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}
