#include <stdio.h>
#include <math.h>

int radio,altura,i,j; 
float x,y,z;
char c,d,e;

void main()
{
    if (1!=2)
    {
        printf("\nIngrese el valor de d = ");
        scanf("%f",&d);
        if (d%2 == 0)
        {
            for (i=0; i<d; i++)
            {
                printf("-");
            }
            printf("\n");
            for (i=d; i>=0; i--)
            {
                printf(" - ",radio);
            }
            printf("\n");
            for (i=0; i<d; i++)
            {
                for (j=0; j<=i; j++)
                {
                    if (j%2==0)
                        printf("+");
                    else
                        printf("-");
                }
                printf("\n");
            }
        }
    }
	printf("\nVariables:");
	printf("\n====================");
	printf("\nradio = %f",radio);
	printf("\naltura = %f",altura);
	printf("\nx = %f",x);
	printf("\ny = %f",y);
	printf("\nz = %f",z);
	printf("\nc = %f",c);
	printf("\nd = %f",d);
	printf("\ne = %f",e);

}