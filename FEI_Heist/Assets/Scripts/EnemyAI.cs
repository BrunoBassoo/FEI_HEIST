using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidadePatrulha = 2f;
    [SerializeField] private bool patrulhaAutomatica = true;
    
    [Header("Configurações de Perseguição")]
    [SerializeField] private float visionRange = 8f; // Range de visão do player
    [SerializeField] private float anguloVisao = 90f; // Ângulo de visão em graus (90 = cone de 90° na frente)
    [SerializeField] private float captureRange = 50f; // Range quando está capturando (maior que vision)
    [SerializeField] private float multiplicadorVelocidade = 3f;
    
    [Header("Configurações de Combate")]
    [SerializeField] private int vidasMaximas = 2; // Inimigo tem 2 vidas
    [SerializeField] private float tempoParalisado = 10f; // Tempo paralisado quando zera vidas
    
    [Header("Música de Perseguição")]
    [SerializeField] private AudioClip musicaPerseguicao;
    [SerializeField] private float volumeMusica = 0.5f;
    private AudioSource audioSource;
    
    [Header("Música de Captura (quando pega o player)")]
    [SerializeField] private AudioClip musicaCaptura;
    [SerializeField] private float volumeMusicaCaptura = 0.7f;
    [SerializeField] private float tempoEsperaAposCaptura = 1.5f; // Tempo antes de resetar a fase
    private AudioSource audioSourceCaptura;
    
    [Header("Sistema de Captura (QTE)")]
    [SerializeField] private KeyCode teclaEscape = KeyCode.E;
    [SerializeField] private int apertosNecessarios = 15;
    [SerializeField] private float tempoLimiteEscape = 5f; // Tempo para apertar E as vezes necessárias
    [SerializeField] private float tempoAtordoamentoAposEscape = 2f; // Tempo que inimigo fica atordoado se player escapar
    
    // Estado do QTE
    private bool emQTE = false;
    private int apertosAtuais = 0;
    private float tempoRestanteQTE = 0f;
    private bool playerCapturado = false;
    
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool usarNavMesh = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;
    
    // Variáveis de controle
    private int vidasAtuais;
    private bool estaInconsciente = false;
    private bool estaPerseguindo = false;
    private Transform playerTransform;
    private PlayerMoviment playerScript;
    
    // Patrulha
    private bool indoDireita = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Tenta pegar o SpriteRenderer no objeto ou em filhos
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        agent = GetComponent<NavMeshAgent>();
        
        // Se não tiver animator atribuído, tenta pegar do componente ou filhos
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // Configura o AudioSource para música de perseguição
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Se não tem, cria um
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurações do AudioSource de perseguição
        audioSource.clip = musicaPerseguicao;
        audioSource.loop = true; // Música fica em loop
        audioSource.playOnAwake = false; // Não toca automaticamente
        audioSource.volume = volumeMusica;
        
        // Cria um segundo AudioSource para música de captura
        audioSourceCaptura = gameObject.AddComponent<AudioSource>();
        audioSourceCaptura.clip = musicaCaptura;
        audioSourceCaptura.loop = false; // Música toca UMA VEZ e termina (Game Over depois)
        audioSourceCaptura.playOnAwake = false;
        audioSourceCaptura.volume = volumeMusicaCaptura;
        
        // Verifica se pegou o SpriteRenderer
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer não encontrado! O flip pode não funcionar corretamente.");
        }
        else
        {
            Debug.Log("SpriteRenderer encontrado!");
        }
        
        vidasAtuais = vidasMaximas;
        
        // Configuração do NavMeshAgent para 2D
        if (agent != null && usarNavMesh)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = velocidadePatrulha;
        }
        
        // Configuração do Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Se usar NavMesh, desativa física do Rigidbody
            if (usarNavMesh && agent != null)
            {
                rb.isKinematic = true;
            }
        }
        
        Debug.Log("Inimigo iniciado com " + vidasAtuais + " vidas");
    }

    void Update()
    {
        // Se está inconsciente, não faz nada
        if (estaInconsciente) return;
        
        // Se já capturou o player, não faz mais nada
        if (playerCapturado) return;
        
        // Se está em QTE, processa o minigame
        if (emQTE)
        {
            ProcessarQTE();
            return;
        }
        
        // Procura pelo player
        ProcurarPlayer();
        
        // Atualiza animações
        AtualizarAnimacoes();
    }
    
    void FixedUpdate()
    {
        // Se está inconsciente, PARA COMPLETAMENTE e não faz nada
        if (estaInconsciente)
        {
            rb.velocity = Vector2.zero; // Garante que está parado
            return;
        }
        
        if (estaPerseguindo && playerTransform != null)
        {
            PerseguirPlayer();
        }
        else
        {
            Patrulhar();
        }
    }
    
    void ProcurarPlayer()
    {
        // Se está em QTE com o player, usa o range de captura (maior)
        // Caso contrário, usa o vision range normal
        float rangeAtual = emQTE ? captureRange : visionRange;
        
        // Detecta todos os objetos no range atual
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(transform.position, rangeAtual);
        
        // Procura pelo player através da tag
        bool playerEncontrado = false;
        foreach (Collider2D obj in objetosDetectados)
        {
            if (obj.CompareTag("Player"))
            {
                // Calcula a direção do player em relação ao inimigo
                Vector2 direcaoParaPlayer = (obj.transform.position - transform.position).normalized;
                
                // Calcula a direção que o inimigo está olhando
                Vector2 direcaoInimigo = indoDireita ? Vector2.right : Vector2.left;
                
                // Calcula o ângulo entre a direção do inimigo e a direção do player
                float angulo = Vector2.Angle(direcaoInimigo, direcaoParaPlayer);
                
                // SE ESTÁ EM QTE, ignora o ângulo (já pegou o player)
                // SE NÃO está em QTE, verifica se o player está no cone de visão
                bool dentroDoAngulo = emQTE || (angulo <= anguloVisao / 2f);
                
                if (!dentroDoAngulo)
                {
                    // Player está ATRÁS ou FORA do cone de visão
                    continue; // Pula para o próximo objeto
                }
                
                // Verifica se o player está visível (não escondido)
                SpriteRenderer playerSprite = obj.GetComponent<SpriteRenderer>();
                bool playerVisivel = playerSprite != null && playerSprite.enabled;
                
                // Só persegue se o player estiver visível E no cone de visão
                if (playerVisivel)
                {
                    // Player detectado e visível NA FRENTE!
                    bool estaVaPerseguir = !estaPerseguindo;
                    estaPerseguindo = true;
                    playerTransform = obj.transform;
                    playerEncontrado = true;
                    
                    // Pega o script do player se não tiver ainda
                    if (playerScript == null)
                    {
                        playerScript = obj.GetComponent<PlayerMoviment>();
                    }
                    
                    // Se acabou de começar a perseguir, toca a música
                    if (estaVaPerseguir)
                    {
                        TocarMusicaPerseguicao();
                        Debug.Log($"👁️ Player detectado! Ângulo: {angulo:F1}° (máx: {anguloVisao/2f}°)");
                    }
                }
                else
                {
                    Debug.Log("👁️ Player escondido! Inimigo não detecta.");
                }
                
                break;
            }
        }
        
        // Se não encontrou o player
        if (!playerEncontrado && estaPerseguindo)
        {
            estaPerseguindo = false;
            playerTransform = null;
            
            // Para a música quando para de perseguir
            PararMusicaPerseguicao();
        }
    }
    
    void Patrulhar()
    {
        if (!patrulhaAutomatica) return;
        
        // Calcula a direção de patrulha
        Vector2 direcao = indoDireita ? Vector2.right : Vector2.left;
        
        // Move o inimigo
        rb.velocity = direcao * velocidadePatrulha;
    }
    
    void PerseguirPlayer()
    {
        if (playerTransform == null) return;
        
        // Calcula a direção até o player
        Vector2 direcao = (playerTransform.position - transform.position).normalized;
        
        // Move em direção ao player com velocidade aumentada
        float velocidadePerseguicao = velocidadePatrulha * multiplicadorVelocidade;
        rb.velocity = direcao * velocidadePerseguicao;
        
        // Flip do sprite baseado na direção
        if (direcao.x > 0 && !indoDireita)
        {
            indoDireita = true;
            Flip();
        }
        else if (direcao.x < 0 && indoDireita)
        {
            indoDireita = false;
            Flip();
        }
    }
    
    void Flip()
    {
        // Vira o sprite baseado na direção
        if (spriteRenderer != null)
        {
            // Se indo para esquerda (false), flipX = true
            // Se indo para direita (true), flipX = false
            spriteRenderer.flipX = !indoDireita;
        }
        
        // Debug para verificar
        Debug.Log("Flip! Indo para direita: " + indoDireita + " | FlipX: " + (spriteRenderer != null ? spriteRenderer.flipX.ToString() : "sem sprite"));
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Se está inconsciente, não faz nada
        if (estaInconsciente) return;
        
        Debug.Log("Inimigo colidiu com: " + collision.gameObject.name + " | Tag: " + collision.gameObject.tag + " | Perseguindo: " + estaPerseguindo);
        
        // Verifica se bateu na parede durante a patrulha
        // SÓ VIRA se NÃO estiver perseguindo E colidiu com algo que NÃO é o player
        if (!estaPerseguindo && !collision.gameObject.CompareTag("Player"))
        {
            // Verifica se realmente bateu em algo sólido (não trigger)
            if (!collision.collider.isTrigger)
            {
                Debug.Log("🧱 BATEU NA PAREDE! Virando...");
                Debug.Log("   Direção ANTES: " + (indoDireita ? "Direita" : "Esquerda"));
                
                // Inverte a direção
                indoDireita = !indoDireita;
                
                Debug.Log("   Direção DEPOIS: " + (indoDireita ? "Direita" : "Esquerda"));
                
                // Faz o flip visual
                Flip();
            }
        }
        
        // Verifica se tocou no player
        if (collision.gameObject.CompareTag("Player"))
        {
            // INICIA O QTE IMEDIATAMENTE quando encostar
            if (!emQTE && !playerCapturado)
            {
                IniciarQTE();
            }
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        // Se player saiu da colisão durante o QTE, cancela (player pode ter sido empurrado)
        if (collision.gameObject.CompareTag("Player") && emQTE)
        {
            Debug.Log("Player saiu da colisão durante QTE!");
            // Não cancela o QTE, apenas registra
        }
    }
    
    void CapturarPlayer()
    {
        playerCapturado = true;
        
        Debug.Log("🚨🚨🚨 PLAYER CAPTURADO! GAME OVER! 🚨🚨🚨");

        // Para o movimento do inimigo
        rb.velocity = Vector2.zero;
        
        // PARA O MOVIMENTO DO PLAYER
        if (playerScript != null)
        {
            playerScript.PararMovimento();
            Debug.Log("❌ Movimento do player desabilitado - capturado!");
        }
        
        // Para o Rigidbody do player também
        if (playerTransform != null)
        {
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }
        }
        
        // Aqui você pode adicionar:
        // - Tela de Game Over
        // - Reiniciar a fase
        // - Voltar ao menu
        // - Mostrar estatísticas
        
        // Por enquanto, pausa o jogo
        StartCoroutine(GameOverComDelay());
        
        // Para a música de perseguição
        PararMusicaPerseguicao();

        
        
        // Toca a música de captura/game over
        TocarMusicaCaptura();
        
        
    }
    
    IEnumerator GameOverComDelay()
    {
        // Espera um tempo curto (configurável) antes de resetar
        Debug.Log($"⏳ Aguardando {tempoEsperaAposCaptura}s antes de resetar a fase...");
        yield return new WaitForSeconds(tempoEsperaAposCaptura);
        
        Debug.Log("💀 Player foi capturado! Resetando fase...");
        
        // Para todas as músicas antes de trocar de cena
        PararMusicaCaptura();
        PararMusicaPerseguicao();
        
        // Garante que o time scale está normal antes de trocar de cena
        Time.timeScale = 1f;
        
        // Chama o Game Manager - PERDE 1 VIDA e REINICIA FASE
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerCapturado();
        }
        else
        {
            Debug.LogError("❌ GameManager não encontrado! Certifique-se de ter o GameManager na cena.");
        }
    }
    
    public void ReceberDano(int dano)
    {
        // Se já está inconsciente, não recebe dano
        if (estaInconsciente) return;
        
        vidasAtuais -= dano;
        
        Debug.Log($"💥 Inimigo recebeu {dano} de dano! Vidas: {vidasAtuais}/{vidasMaximas}");
        
        // Se estava no QTE, cancela (player conseguiu atacar!)
        if (emQTE)
        {
            emQTE = false;
            apertosAtuais = 0;
            
            // Libera o player
            if (playerScript != null)
            {
                playerScript.LiberarMovimento();
            }
            
            Debug.Log("Inimigo foi atingido durante QTE! Player escapou!");
        }
        
        // Animação de hit (se tiver)
        // NOTA: Descomente se você tiver o parâmetro "Hit" (Trigger) no Animator:
        // if (animator != null)
        // {
        //     animator.SetTrigger("Hit");
        // }
        
        // Verifica se zerou as vidas
        if (vidasAtuais <= 0)
        {
            FicarParalisado();
        }
    }
    
    void FicarParalisado()
    {
        estaInconsciente = true;
        vidasAtuais = 0; // Garante que está zerado
        
        // PARA TODOS OS MOVIMENTOS
        rb.velocity = Vector2.zero;
        
        // Para de perseguir o player
        estaPerseguindo = false;
        playerTransform = null;
        
        // Cancela QTE se estiver ativo
        if (emQTE)
        {
            emQTE = false;
            apertosAtuais = 0;
            
            // Libera o player
            if (playerScript != null)
            {
                playerScript.LiberarMovimento();
            }
        }
        
        // Para todas as músicas
        PararMusicaPerseguicao();
        PararMusicaCaptura();
        
        Debug.Log($"💀 Inimigo foi derrotado! Ficará paralisado por {tempoParalisado} segundos!");
        
        // Animação de inconsciente (se tiver)
        // NOTA: Descomente as linhas abaixo se você tiver esses parâmetros no Animator:
        // if (animator != null)
        // {
        //     animator.SetBool("isInconsciente", true);
        //     animator.SetBool("isMoving", false);
        //     animator.SetBool("isPerseguindo", false);
        // }
        
        // Muda a cor para indicar que está paralisado (cinza transparente)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
        
        // Inicia a corrotina para recuperar
        StartCoroutine(RecuperarDepoisDeTempo());
    }
    
    IEnumerator RecuperarDepoisDeTempo()
    {
        // Durante todo o tempo paralisado, mantém parado
        float tempoDecorrido = 0f;
        
        Debug.Log($"⏱️ Inimigo paralisado... ({tempoParalisado}s)");
        
        while (tempoDecorrido < tempoParalisado)
        {
            rb.velocity = Vector2.zero; // Garante que continua parado
            tempoDecorrido += Time.deltaTime;
            
            // Mostra contagem regressiva a cada segundo
            if (Mathf.FloorToInt(tempoDecorrido) != Mathf.FloorToInt(tempoDecorrido - Time.deltaTime))
            {
                float tempoRestante = tempoParalisado - tempoDecorrido;
                Debug.Log($"⏱️ Inimigo se recupera em: {tempoRestante:F0}s");
            }
            
            yield return null;
        }
        
        // Recupera o inimigo
        estaInconsciente = false;
        vidasAtuais = vidasMaximas; // Restaura as 2 vidas
        
        Debug.Log($"✅ Inimigo recuperado! Vidas restauradas: {vidasAtuais}/{vidasMaximas}");
        
        // Volta às animações normais
        // NOTA: Descomente se você tiver o parâmetro "isInconsciente" no Animator:
        // if (animator != null)
        // {
        //     animator.SetBool("isInconsciente", false);
        // }
        
        // Restaura a cor normal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    
    void AtualizarAnimacoes()
    {
        if (animator == null) return;
        
        // NOTA: Se você tiver os parâmetros "isMoving" e "isPerseguindo" no Animator,
        // descomente as linhas abaixo:
        
        // Verifica se está se movendo
        // bool estaMovendo = rb.velocity.magnitude > 0.1f;
        // animator.SetBool("isMoving", estaMovendo);
        
        // Define se está perseguindo
        // animator.SetBool("isPerseguindo", estaPerseguindo);
    }
    
    // Método público para ser chamado quando o player ataca
    public void SerAtacadoPeloPlayer(int dano)
    {
        ReceberDano(dano);
    }
    
    // Getters
    public int GetVidas()
    {
        return vidasAtuais;
    }
    
    public bool EstaInconsciente()
    {
        return estaInconsciente;
    }
    
    public bool EstaTocandoPlayer()
    {
        // Retorna true se está em QTE (compatibilidade com UI antiga)
        return emQTE;
    }
    
    public float GetTempoSegurandoPlayer()
    {
        // Retorna o tempo restante do QTE (para compatibilidade com UI antiga)
        return tempoRestanteQTE;
    }
    
    public float GetTempoParaCapturar()
    {
        return tempoLimiteEscape;
    }
    
    public float GetProgressoCaptura()
    {
        // Retorna o progresso do QTE (quantos % já apertou)
        if (emQTE)
        {
            return (float)apertosAtuais / apertosNecessarios;
        }
        return 0f;
    }
    
    // Novos getters para o sistema de QTE
    public int GetApertosAtuais()
    {
        return apertosAtuais;
    }
    
    public int GetApertosNecessarios()
    {
        return apertosNecessarios;
    }
    
    public float GetTempoRestanteQTE()
    {
        return tempoRestanteQTE;
    }
    
    public bool EstaEmQTE()
    {
        return emQTE;
    }
    
    public KeyCode GetTeclaEscape()
    {
        return teclaEscape;
    }
    
    // ======================== SISTEMA DE MÚSICA ========================
    
    void TocarMusicaPerseguicao()
    {
        // Só toca se tiver música configurada e um AudioSource
        if (audioSource != null && musicaPerseguicao != null)
        {
            // Se já não estiver tocando
            if (!audioSource.isPlaying)
            {
                // Notifica o MusicManager que uma música vai tocar
                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.RegistrarMusicaAtiva();
                }
                
                audioSource.Play();
                Debug.Log("🎵 Música de perseguição iniciada!");
            }
        }
    }
    
    void PararMusicaPerseguicao()
    {
        // Para a música se estiver tocando
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            
            // Notifica o MusicManager que a música parou
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.DesregistrarMusicaAtiva();
            }
            
            Debug.Log("🎵 Música de perseguição parada!");
        }
    }
    
    void TocarMusicaCaptura()
    {
        // Só toca se tiver música configurada
        if (audioSourceCaptura != null && musicaCaptura != null)
        {
            // Se já não estiver tocando
            if (!audioSourceCaptura.isPlaying)
            {
                // Notifica o MusicManager que uma música vai tocar
                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.RegistrarMusicaAtiva();
                }
                
                audioSourceCaptura.Play();
                Debug.Log("🚨 Música de CAPTURA iniciada! Player foi pego!");
            }
        }
    }
    
    void PararMusicaCaptura()
    {
        // Para a música se estiver tocando
        if (audioSourceCaptura != null && audioSourceCaptura.isPlaying)
        {
            audioSourceCaptura.Stop();
            
            // Notifica o MusicManager que a música parou
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.DesregistrarMusicaAtiva();
            }
            
            Debug.Log("🚨 Música de captura parada! Player escapou!");
        }
    }
    
    // ==================== SISTEMA DE QTE (INTEGRADO) ====================
    
    void IniciarQTE()
    {
        emQTE = true;
        apertosAtuais = 0;
        tempoRestanteQTE = tempoLimiteEscape;
        
        Debug.Log($"🎮 QTE INICIADO! Aperte [{teclaEscape}] {apertosNecessarios} vezes em {tempoLimiteEscape} segundos!");
        
        // PARA O INIMIGO (fica "colado" no player)
        rb.velocity = Vector2.zero;
        estaPerseguindo = false; // Para de perseguir durante QTE
        
        // PARA O PLAYER
        if (playerScript != null)
        {
            playerScript.PararMovimento();
            Debug.Log("❌ Player travado! Não pode se mover nem atacar!");
        }
        
        // MOSTRA A UI DO QTE
        if (QTE_UI.Instance != null)
        {
            QTE_UI.Instance.MostrarUI(this);
        }
        
        // Para a música de perseguição
        PararMusicaPerseguicao();
        
        // Toca música de captura
        TocarMusicaCaptura();
    }
    
    void ProcessarQTE()
    {
        // INIMIGO GRUDA NO PLAYER (segue a posição)
        if (playerTransform != null)
        {
            // Mantém o inimigo colado no player
            Vector2 offsetPosicao = new Vector2(0.3f, 0); // Pequeno offset para não ficar exatamente em cima
            transform.position = Vector2.Lerp(transform.position, (Vector2)playerTransform.position + offsetPosicao, Time.deltaTime * 10f);
            
            // Garante que está parado (sem velocity)
            rb.velocity = Vector2.zero;
        }
        
        // Atualiza o timer
        tempoRestanteQTE -= Time.deltaTime;
        
        // Verifica se apertou a tecla
        if (Input.GetKeyDown(teclaEscape))
        {
            apertosAtuais++;
            Debug.Log($"⌨️ Apertou! {apertosAtuais}/{apertosNecessarios} | Tempo: {tempoRestanteQTE:F1}s");
            
            // Verifica se completou
            if (apertosAtuais >= apertosNecessarios)
            {
                PlayerEscapouDoQTE();
                return;
            }
        }
        
        // Verifica se o tempo acabou
        if (tempoRestanteQTE <= 0)
        {
            PlayerFalhouNoQTE();
        }
    }
    
    void PlayerEscapouDoQTE()
    {
        emQTE = false;
        
        Debug.Log("✅ PLAYER ESCAPOU! Conseguiu apertar todas as vezes!");
        
        // ESCONDE A UI
        if (QTE_UI.Instance != null)
        {
            QTE_UI.Instance.EsconderUI();
        }
        
        // LIBERA O PLAYER
        if (playerScript != null)
        {
            playerScript.LiberarMovimento();
            Debug.Log("✅ Player liberado! Pode se mover novamente!");
        }
        
        // Para a música de captura
        PararMusicaCaptura();
        
        // EMPURRA O INIMIGO PARA TRÁS (knockback)
        if (playerTransform != null)
        {
            Vector2 direcao = (transform.position - playerTransform.position).normalized;
            rb.velocity = direcao * velocidadePatrulha * 3f; // Empurra para trás com força
        }
        
        // ATORDOA O INIMIGO
        StartCoroutine(AtordoarInimigo());
    }
    
    IEnumerator AtordoarInimigo()
    {
        estaInconsciente = true; // Usa o sistema de inconsciente já existente
        
        Debug.Log($"😵 Inimigo ATORDOADO por {tempoAtordoamentoAposEscape} segundos!");
        
        // Muda a cor para indicar atordoamento
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.7f, 0.7f, 1f, 0.8f); // Azulado
        }
        
        // Para completamente
        rb.velocity = Vector2.zero;
        
        // Espera o tempo de atordoamento
        yield return new WaitForSeconds(tempoAtordoamentoAposEscape);
        
        // Recupera
        estaInconsciente = false;
        
        Debug.Log("💪 Inimigo recuperou do atordoamento!");
        
        // Restaura a cor
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        // Volta a perseguir se o player ainda estiver no range
        if (playerTransform != null)
        {
            float distancia = Vector2.Distance(transform.position, playerTransform.position);
            if (distancia <= visionRange)
            {
                estaPerseguindo = true;
            }
        }
    }
    
    void PlayerFalhouNoQTE()
    {
        emQTE = false;
        
        Debug.Log("❌ PLAYER FALHOU! Não apertou rápido o suficiente!");
        
        // ESCONDE A UI
        if (QTE_UI.Instance != null)
        {
            QTE_UI.Instance.EsconderUI();
        }
        
        // Player não conseguiu escapar, é capturado
        CapturarPlayer();
    }
    
    // Visualização dos ranges no Editor
    void OnDrawGizmosSelected()
    {
        // Direção que o inimigo está olhando
        Vector3 direcao = indoDireita ? Vector3.right : Vector3.left;
        
        // CONE DE VISÃO (amarelo)
        Gizmos.color = Color.yellow;
        
        // Desenha o cone de visão
        float anguloInicial = -anguloVisao / 2f;
        float anguloFinal = anguloVisao / 2f;
        int segmentos = 20; // Quantas linhas para desenhar o cone
        
        Vector3 posicaoAnterior = transform.position;
        
        for (int i = 0; i <= segmentos; i++)
        {
            float anguloAtual = anguloInicial + (anguloFinal - anguloInicial) * i / segmentos;
            
            // Rotaciona a direção pelo ângulo
            float radianos = anguloAtual * Mathf.Deg2Rad;
            Vector3 direcaoRotacionada = new Vector3(
                direcao.x * Mathf.Cos(radianos) - direcao.y * Mathf.Sin(radianos),
                direcao.x * Mathf.Sin(radianos) + direcao.y * Mathf.Cos(radianos),
                0
            );
            
            Vector3 pontoNoCone = transform.position + direcaoRotacionada * visionRange;
            
            // Desenha linha do centro para o ponto
            Gizmos.DrawLine(transform.position, pontoNoCone);
            
            // Desenha linha conectando os pontos (forma o arco)
            if (i > 0)
            {
                Gizmos.DrawLine(posicaoAnterior, pontoNoCone);
            }
            
            posicaoAnterior = pontoNoCone;
        }
        
        // Desenha linha do range de captura (vermelho) - círculo completo quando em QTE
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, captureRange);
    }
}

