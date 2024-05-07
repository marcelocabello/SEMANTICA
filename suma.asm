section .data:
; VARIABLES
radio dw 0
altura dw 0
i dw 0
j dw 0
x dd 0
y dd 0
z dd 0
c db 0
d db 0
e db 0
section .text
	 global _start

_start:

;IF
mov eax,1
push eax
mov eax,2
push eax
pop ebx
pop eax
cmp eax, ebx
je EtiquetaIF1
;LECTURA 1: 

mov eax, 3 
mov ebx, 2 
lea ecx, [d]
mov edx, 1
int 0x80

;IF
mov eax,2
push eax
pop ebx
pop eax
idiv ebx
push edx
mov eax,0
push eax
pop ebx
pop eax
cmp eax, ebx
jne EtiquetaIF2
;FOR
EtiquetaFOR1:
mov eax,0
push eax
pop eax
pop ebx
pop eax
cmp eax, ebx
jge EtiquetafinFOR1
mov eax, [i]
inc eax
mov [i], eax
jmp EtiquetaFOR1
EtiquetafinFOR1:
;FOR
EtiquetaFOR2:
pop eax
mov eax,0
push eax
pop ebx
pop eax
cmp eax, ebx
jl EtiquetafinFOR2
mov eax, [i]
dec eax
mov [i], eax
jmp EtiquetaFOR2
EtiquetafinFOR2:
;FOR
EtiquetaFOR3:
mov eax,0
push eax
pop eax
pop ebx
pop eax
cmp eax, ebx
jge EtiquetafinFOR3
mov eax, [i]
inc eax
mov [i], eax
;FOR
EtiquetaFOR4:
mov eax,0
push eax
pop eax
pop ebx
pop eax
cmp eax, ebx
jg EtiquetafinFOR4
mov eax, [j]
inc eax
mov [j], eax
;IF
mov eax,2
push eax
pop ebx
pop eax
idiv ebx
push edx
mov eax,0
push eax
pop ebx
pop eax
cmp eax, ebx
jne EtiquetaIF3
jmp EtiquetafinIF3
EtiquetaIF3:
EtiquetafinIF3:
jmp EtiquetaFOR4
EtiquetafinFOR4:
jmp EtiquetaFOR3
EtiquetafinFOR3:
jmp EtiquetafinIF2
EtiquetaIF2:
EtiquetafinIF2:
jmp EtiquetafinIF1
EtiquetaIF1:
EtiquetafinIF1:
;FIN PROGRAMA:

mov eax, 1
xor ebx, ebx 
int 0x80
