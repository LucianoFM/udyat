using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Udyat.Class
{
    public class SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            // Estado para escondendo
            State state = State.Hiding;

            // Índice dos caracter que será escondido
            int charIndex = 0;

            // Contém o valor do caracter convertido para inteiro
            int charValue = 0;

            // Contém o índice do elemento de cor (R ou G ou B) que está sendo processado atualmente
            long pixelElementIndex = 0;

            // Contém o número de zeros à esquerda que foram adicionados ao finalizar o processo
            int zeros = 0;

            // Elementos do Pixel
            int R = 0, G = 0, B = 0;

            // Varre as linhas
            for (int i = 0; i < bmp.Height; i++)
            {
                // Varre as colunas
                for (int j = 0; j < bmp.Width; j++)
                {
                    // Pixel que está sendo processado
                    Color pixel = bmp.GetPixel(j, i);

                    // Limpa o pixel com Bit Menos Significante (LSB) de cada elemento do pixel
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // Para cada elemento do pixel (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        if (pixelElementIndex % 8 == 0)
                        {
                            // Verifica se o processo finalizou (quando 8 zeros foram adicionados)
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // Aplica o último pixel na imagem
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                // Retorna o bitmap com o texto escondido
                                return bmp;
                            }

                            // Verifica se todos os caracteres foram escondidos
                            if (charIndex >= text.Length)
                            {
                                // Comoçe a adicionar zeros para marcar o fim do texto
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // Move para o próximo caracter
                                charValue = text[charIndex++];
                            }
                        }

                        // Verifica qual elemento do pixel tem a capacidade de esconder um bit menos significativo LSB
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // O bit mais a direita será (charValue % 2)
                                        R += charValue % 2;

                                        // Remove o bit mais a direita do caracter
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            // Incrementa o total de zeros (até 8)
                            zeros++;
                        }
                    }
                }
            }
            return bmp;
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            // Contém o texto que será extraído da imagem
            string extractedText = string.Empty;

            // Varre as linhas
            for (int i = 0; i < bmp.Height; i++)
            {
                // varre as colunas
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // Varre os elementos do pixel
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // pega o LSB a partir do elemento pixel (será pixel.R% 2)
                                    // e então adiciona um bit à direita do caracter atual
                                    // Isso pode ser feito por (charValue = charValue * 2)
                                    // Substitui o bit adicionado (o qual o valor é, por padrão, 0) com
                                    // o LSB do elemento de pixel, simplesmente por adição
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        // Se foi adicionado 8 bits
                        // então adiciona o caractere atual ao texto do resultado
                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            // Converte o valor do caracter de int para string
                            char c = (char)charValue;
                            // Adiciona o caracter ao texto
                            extractedText += c.ToString();
                        }
                    }
                }
            }
            return extractedText;
        }
        
        public static int reverseBits(int n)
        {
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;
                n /= 2;
            }
            return result;
        }

    }    

}
