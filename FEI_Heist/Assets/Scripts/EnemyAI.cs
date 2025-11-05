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
    [SerializeField] private float visionRange = 5f; // Range de visão/captura do player
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
    private AudioSource audioSourceCaptura;
    
    [Header("Timer de Captura (Game Over)")]
    [SerializeField] private float tempoParaCapturar = 3f; // 3 segundos até game over
    private float tempoSegurandoPlayer = 0f;
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
    private bool estaTocandoPlayer = false;
    
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
        audioSourceCaptura.loop = true; // Música fica em loop enquanto segura o player
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
        
        // Procura pelo player
        ProcurarPlayer();
        
        // Verifica se ainda está no range (para esconder mensagem se escapar)
        VerificarRangePlayer();
        
        // Atualiza o timer de captura se estiver segurando o player
        AtualizarTimerCaptura();
        
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
        // Detecta todos os objetos no vision range
        Collider2D[] objetosDetectados = Physics2D.OverlapCircleAll(transform.position, visionRange);
        
        // Procura pelo player através da tag
        bool playerEncontrado = false;
        foreach (Collider2D obj in objetosDetectados)
        {
            if (obj.CompareTag("Player"))
            {
                // Verifica se o player está visível (não escondido)
                SpriteRenderer playerSprite = obj.GetComponent<SpriteRenderer>();
                bool playerVisivel = playerSprite != null && playerSprite.enabled;
                
                // Só persegue se o player estiver visível
                if (playerVisivel)
                {
                    // Player detectado e visível!
                    bool estaVaPerseguir = !estaPerseguindo;
                    estaPerseguindo = true;
                    playerTransform = obj.transform;
                    playerEncontrado = true;
                    
                    // Se acabou de começar a perseguir, toca a música
                    if (estaVaPerseguir)
                    {
                        TocarMusicaPerseguicao();
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
        if (collision.gameObject.CompareTag("Paredes") && !estaPerseguindo)
        {
            Debug.Log("Direção ANTES: indoDireita = " + indoDireita);
            
            // Inverte a direção
            indoDireita = !indoDireita;
            
            Debug.Log("Direção DEPOIS: indoDireita = " + indoDireita);
            
            Flip();
            Debug.Log("Inimigo bateu na parede e virou!");
        }
        
        // Verifica se tocou no player
        if (collision.gameObject.CompareTag("Player"))
        {
            estaTocandoPlayer = true;
            Debug.Log("Tempo restante para capturar: " + tempoParaCapturar + " segundos!!");
            // Música de perseguição CONTINUA tocando!
            // Timer vai começar a contar no Update()
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        // Se player saiu da colisão
        if (collision.gameObject.CompareTag("Player"))
        {
            estaTocandoPlayer = false;
            tempoSegurandoPlayer = 0f; // Reseta o timer
            Debug.Log("Player escapou da colisão! Timer resetado.");
        }
    }
    
    void VerificarRangePlayer()
    {
        // Se estava tocando o player mas perdeu ele de vista (saiu do range)
        if (estaTocandoPlayer && !estaPerseguindo)
        {
            estaTocandoPlayer = false;
            tempoSegurandoPlayer = 0f; // Reseta o timer
            Debug.Log("Player escapou do range! Timer resetado.");
        }
    }
    
    void AtualizarTimerCaptura()
    {
        // Se está tocando o player, conta o tempo
        if (estaTocandoPlayer && !playerCapturado)
        {
            tempoSegurandoPlayer += Time.deltaTime;
            
            // Debug para ver o progresso
            if (tempoSegurandoPlayer % 1f < Time.deltaTime) // A cada segundo aproximadamente
            {
                Debug.Log($"⏱️ Segurando player: {tempoSegurandoPlayer:F1}s / {tempoParaCapturar}s");
            }
            
            // Verifica se o tempo acabou
            if (tempoSegurandoPlayer >= tempoParaCapturar)
            {
                CapturarPlayer();
            }
        }
    }
    
    void CapturarPlayer()
    {
        playerCapturado = true;
        
        Debug.Log("🚨🚨🚨 PLAYER CAPTURADO! GAME OVER! 🚨🚨🚨");

        // Para o movimento do inimigo
        rb.velocity = Vector2.zero;
        
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
        // Espera um pouco para a música tocar
        yield return new WaitForSeconds(2f);
        
        // Pausa o jogo
        Time.timeScale = 0f;
        
        Debug.Log("Jogo pausado. Pressione 'R' para reiniciar (você precisa adicionar essa funcionalidade)");
    }
    
    public void ReceberDano(int dano)
    {
        // Se já está inconsciente, não recebe dano
        if (estaInconsciente) return;
        
        vidasAtuais -= dano;
        
        Debug.Log($"💥 Inimigo recebeu {dano} de dano! Vidas: {vidasAtuais}/{vidasMaximas}");
        
        // Se estava tocando o player, para de tocar (foi atingido)
        if (estaTocandoPlayer)
        {
            estaTocandoPlayer = false;
            tempoSegurandoPlayer = 0f; // Reseta o timer de captura
            Debug.Log("Inimigo foi atingido! Player escapou!");
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
        estaTocandoPlayer = false;
        
        // Reseta o timer de captura (player escapou!)
        tempoSegurandoPlayer = 0f;
        
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
        return estaTocandoPlayer;
    }
    
    public float GetTempoSegurandoPlayer()
    {
        return tempoSegurandoPlayer;
    }
    
    public float GetTempoParaCapturar()
    {
        return tempoParaCapturar;
    }
    
    public float GetProgressoCaptura()
    {
        return tempoSegurandoPlayer / tempoParaCapturar;
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
    
    // Visualização do vision range no Editor
    void OnDrawGizmosSelected()
    {
        // Vision range (amarelo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}

