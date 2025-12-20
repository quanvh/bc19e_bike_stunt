using UnityEngine;

namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public static class UtilsCollider
    {
        public struct NormalAndPointStruct
        {
            public Vector2 Normal;
            public Vector2 Point;
        }

        public static NormalAndPointStruct GetContactNormalAndPointAVG( Collider2D collider, int maxNumOfContacts = 10 )
        {
            var normalAndPoint = new NormalAndPointStruct();
            normalAndPoint.Normal = Vector2.zero;
            normalAndPoint.Point = Vector2.zero;

            var contactsArray = new ContactPoint2D[maxNumOfContacts];
            int numOfContacts = collider.GetContacts(contactsArray);
            if (numOfContacts > 0)
            {
                for (int i = 0; i < numOfContacts; ++i)
                {
                    normalAndPoint.Normal += contactsArray[i].normal;
                    normalAndPoint.Point += contactsArray[i].point;
                }
                normalAndPoint.Normal /= numOfContacts;
                normalAndPoint.Point /= numOfContacts;
            }

            return normalAndPoint;
        }
    }
}
