section .text
	 global _start
mov eax, [3]
push eax
mov eax, [5]
push eax
pop ebx
pop eax
add eax, eax
push ebx
mov eax, [8]
push eax
pop ebx
pop eax
mul ebx
push eax
mov eax, [10]
push eax
mov eax, [4]
push eax
pop ebx
pop eax
sub eax, ebx
push eax
mov eax, [2]
push eax
pop ebx
pop eax
div ebx
push eax
pop ebx
pop eax
sub eax, ebx
push eax
pop eax
mov [radio], eax
mov eax, [10]
push eax
pop ebx
pop ebx
cmp eax, ebx
jle EtiquetaIF1:
EtiquetaDO1:
mov eax, [1]
push eax
pop ebx
pop eax
add eax, eax
push ebx
pop eax
mov [pi], eax
inc dword [pi]
mov [pi], eax
mov eax, [10]
push eax
pop ebx
pop ebx
cmp eax, ebx
jle EtiquetaDO1
EtiquetaIF1:
ret
section .data
; VARIABLES
x dw 0
radio dw 0
pi dw 0
