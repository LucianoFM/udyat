# udyat
Udyat is a simple command line system that after installed will take pictures from all monitor on computer 

# Ini file


 Linha de comando:

- Udyat -vi [nome da imagem] 
Verificar se uma imagem possui assinatura do Udyat.
Resultado: adiciona no log local a informação se a imagem é assinada ou não.
- Udyat -quit
Finaliza o processo.  
Resultado: encerra o sistema após capturar a próxima imagem.
- Udyat -install
Instala o sistema com as configurações contidas no arquivo .ini
Depois de instalado: é possível apagar o arquivo .ini para evitar que usuários possam modificá-lo
- Udyat -uninstall
Desinstala o sistema. Não invalida a linceça. É possível reinstalar a licença (na mesma máquina).
Para reinstalar é obrigatório o arquivo .ini 





 Características básicas

- Contém: Executável "Udyat.exe" de 155kb e 1 arquivo "udyat.ini"
- Sistema ofuscado para evitar engenharia reversa e roubo de código fonte
- Deve excutar em Windows 64 bits a partir do Windows 7 (deve ser testado em 32bits e, se necessário, gerar um executável específico)
- A máquina onde o sistema será executado deve possuir acesso a internet 
  - Exige conexão:
    - ao iniciar o sistema     
      - a cada virada de dia (se o sistema rodar mais do que 24h)
  - Possui opção para conexão com proxy
  - Possui configuração para especificar o número de tentativas de conexão com a internet antes de finalizar se não houver conexão
- Exige o arquivo .INI de configuração para a instalação
  - Depois de instalar: é possível apagar o .ini (o sistema continuará sendo executado, normalmente)   
- É exibido nos processos do Windows
- Pode ser configurado para iniciar junto com o Windows
- Não deve exibir exceções com mensagem de tela (exceções e erros são salvos em log)
- Grava log (por dia) na pasta temporária de geração das imagens
- Possui relógio interno (evita problemas se o usuário trocar data e/ou hora do computador)
  - Sincroniza o horário com servidor brasileiro de horário
  - Se não conseguir conexão com o servidor de horário, sincroniza com um serviço do geolicenses
  - Em casos de excessão: utiliza o horário da máquina
- Possui modo de teste (execução do sistema para teste) e produção (execução real)
  - Mesmo versão de teste exige licença (TRIAL ou quente)
- Para executar o sistema é necessário uma chave de licença
- Outras características podem ser observadas ao ler as configurações do arquivo .INI 





 Instruções para instalação



1. Acessar www.geolicenses.com (fiz em PHP mas descontinuei -- caso queira o código me envia um email)
   2. Logon
   3. Existem Três opções:
      1. Representantes (já tem cadastrado Panorama)
      2. Clientes (clientes de um representante [ou seja: pra quem aquele representante vendeu linceça])
      3. Licenças (cadastro de licenças de produção e trial)
   4. Para criar uma licença: 
      1. Acessar menu Licenças > escolher o Representante > escolher o Cliente > Criar licença
   5. Copiar a licença (Serial Key) gerada, inserir a Serial Key na chave "serialkey" do arquivo .INI e salvar
   6. Instalar o Udyat conforme opções desejadas (instalação: Udyat -install)
   


 CONNECTION

 ATTEMPTS
    Número que indica a quantidade de tentativas de conexão quando o sistema não consegue se conectar na internet
    O sistema é finalizado depois que o número de tentativas é ultrapassado
    Se o número for 0 (zero), então o sistema não faz qualquer tentativa e finaliza.
    Se o número for 999 então o sistema continua tentanto (o sistema não é finalizado)

 PROXY
    on = Ativa a conexão via proxy 
    off = Não utiliza proxy para a conexão com internet

 HOST
    Host ou IP do servidor de proxy que é utilizado para acesso com a internet
    A política de acesso a sites deve permitir o acesso ao site: geolicenses.com

 PORT
   Número da porta do proxy 

 DEFAULTAUTH
   on = utiliza as credenciais padrão (utiliza usuário, senha e domínio do usuário logado no Windows) 
   off = não utiliza as credenciais padrão. Nesse caso o sistema procura pelas credenciais informadas nas chaves DOMAIN, USER e PASSWORD

 DOMAIN
   Nome do domínio de rede que o usuário do proxy irá utilizar

 USER
   Nome do usuário de rede que será logado no proxy
 
 PASSWORD
   Senha do usuário de rede que será logado no proxy 


[CONNECTION]
attempts=100
proxy=off
host=none
port=none
defaultAuth=off
domain=none
user=none
password=none


 EXECUTION

 MODE
    0: produção
    1: teste (captura 10 telas e finaliza automaticamente)

 LOCALLOG
    on: mantém log de erros e de início e fim do processamento (arquivo "YYYYMMDD_UdyatLog.txt" na pasta de armazenamento temporário das imagens)
    off: não mantém o log local

 CLOCK
    Tempo em milesegundos para o processamento do relógio interno do sistema (Ex: 5 segundos = 5000)

 AUTODIR
    Indica se a aplicação irá utilizar ou diretório temporário (on) ou um personalizado (off) para
    armazenar as imagens temporariamente
    on = diretório temporário do usuário no windows (automático) (...pasta temporária do usuário\udyat)
    off = utiliza a pasta da chave PROCDIR

 PROCDIR
    Caminho de armazenamento temporário das imagens capturadas
    O diretório deve existir, pois o sistema não cria. Se não existir: o modo automático é ativado (AUTODIR=on).

 MOVEFILE
    on = Indica que é para mover as imagens capturadas para uma pasta 
    off = não move as imagens capturadas (ficam mantidas na pasta temporária do usuário ou na pasta especificada na chave PROCDIR)

 MOVEAFTER
    Número de imagens que devem ser capturadas antes que o sistema envie as imagens para a pasta configurada na chave TARGET
    Funciona somente se a chave MOVEFILE = on
    O valor padrão é 10

 TARGET
    Pasta destino onde as imagens capturadas serão armazenadas. 
    Funciona somente se a chave MOVEFILE = on
    Se o valor for "default", então as imagens capturadas serão armazenadas em uma pasta chamada "output" (no diretório do executável)

 WSTARTUP
    on = inicia a aplicação sempre que o Windows for reiniciado
    off = não inicia a aplicação após o início do Windows 


[EXECUTION]
mode=0
locallog=on
clock=5000
autodir=on
procdir=c:\udyat\imgs
movefile=on
moveafter=30
target=default
wstartup=off


 CAPTURE

 INTERVAL
    Tempo em milesegundos para o disparo da thread que realiza o print
    Padrão é 2000

 IMGTYPE
    Tipo de imagem que será salva
    1 = JPG
    2 = PNG (padrão) (somente esse formato pode gravar a assinatura da imagem -- chave "signature")
    3 = GIF

 JPGQUALITY
    Número entre 0 e 100 que representa a qualidade da imagem (próximo de 0 = menor qualidade, próximo de 100 = maior qualidade)
    funciona somente quando a chave IMGTYPE = 1 (jpg)

 LEGEND
    on = adiciona legenda em cada imagen capturada
    off = não adiciona legenda

 NTPSERVER
    Endereço do Servidor de horário utilizado para ajustar o relógio interno
    Padrão: a.ntp.br (Servidor nacional com melhor latência)
    Para identificar se o sistema está buscando o horário do NTP: verificar o log local, onde deve constar o texto "Relógio Global" no log "Inicializando relógio"

 NTPPORT
    Porta do NTPSERVER
    A porta padrão de servidores NTP é 123 (padrão)

 PERSCREEN 
    on = Captura 1 imagem por monitor
    off = Captura a imagem de todos monitores em uma única imagem

 SIGNATURE
    on = Esconde os dados abaixo em cada imagem capturada (não aumenta o tamanho da imagem, pois é utilizado uma técnica de esteganografia que utiliza os bits menos significativos (LSB) para salvar 
         os dados nos pixels que possuem LSB):
            1) Hash da palavra chave do cliente (garante que a imagem foi feita pelo sistema e para um cliente válido) (a palavra chave do cliente depende da informação de ID adicionada na chave CUSTOMER)
            2) Identificador Único da Máquina onde o sistema está executando (garante que a execução foi feita a partir de uma máquina com licença válida)
            3) Nome do arquivo (garante a identidade da imagem)
                  - No do arquivo
                  - Usuário logado na máquina no momento do print 
                  - MAC Address da máquina
                  - IP
                  - Data e hora do print (no formato dd/mm/yyyy hh:mm:ss fff)
         A palavra chave do cliente é resgatada via internet, com acesso a um serviço do site e através do ID do cliente, especificado aqui através da chave CUSTOMER
         Caso não exista o ID do cliente ou a palavra chave ou ausência de internet: o sistema será finalizado sem fazer nada
         Se o modo de execução for Teste: a palavra chave será "Teste Udyat" e não é necessário ID de cliente na chave CUSTOMER
    off = não adiciona os dados de assinatura nas imagens


[CAPTURE]
interval=2000
imgtype=1
jpgquality=40
legend=off
ntpserver=a.ntp.br
ntpport=123
perscreen=on
signature=on



 SERIALKEY 
    Número de validação e ativação do sistema

 LICADDRESS
    Endereço para o qual a licença foi comprada
    Esse endereço deve ser informado em casos onde o IP da máquina local não corresponde ao endereço da licença
    É recomendado que o endereço esteja no formato: [ Logradouro ] [ Endereço ], [ Número ] [ Cidade ] [ País ]


[CUSTOMER]
serialkey=colocar a licença aqui ao invés desse texto
licaddress=Rua da licença, 1001
