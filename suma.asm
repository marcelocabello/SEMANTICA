section .data:
; VARIABLES
radio dw 0
section .text
	 global _start

_start:

EtiquetaDO1:
;LECTURA 1: 

mov eax, 3 
mov ebx, 2 
lea ecx, [radio]
mov edx, 5
int 0x80

mov eax,3
push eax
pop ebx
pop ebx
cmp eax, ebx
jne EtiquetaDO1
;LECTURA 2: 

mov eax, 3 
mov ebx, 2 
lea ecx, [radio]
mov edx, 5
int 0x80

mov eax,3
push eax
pop ebx
pop ebx
cmp eax, ebx
jne EtiquetaDO1
jmp EtiquetaDO1
;FIN PROGRAMA:

mov eax, 1
xor ebx, ebx 
int 0x80
