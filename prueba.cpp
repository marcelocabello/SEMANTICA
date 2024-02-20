#include <stdio.h>
#include <math.h>
int altura,i,j,x26;
float x,c,z;
char y,a;

void main()
{
    x=257;//tiene que dar el valor
    y=(char)x;
    printf("El valor\nde y =",y);
    printf("\nInserte el valor de i\n");
    scanf("%f",&i);
    i -=4;
    printf("El valor de i -=4 : ",i);
    i++;
    printf("\nEl valor de i++ = ",i);
    i--;
    printf("\nEl valor de i--= ",i);
    x+=i;
    printf("\nEl valor de x+=i : ",x);
    printf("\nEl valor de z = ",z);
}