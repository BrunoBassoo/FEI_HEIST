using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoviment : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float velocidadeNormal = 5f;
    [SerializeField] private float velocidadeLenta = 2f;
    [SerializeField] private bool isRunning = true;
    private float velocidadeAtual;
    
    [Header("Configurações de Combate")]
    [SerializeField] private float delayAtaque = 0.5f;
    [SerializeField] private float rangeAtaque = 2f;
    [SerializeField] private int danoAtaque = 1;
    
    [Header("Sistema de Chaves")]
    [SerializeField] private int chaves = 0;
    
    [Header("Sistema de Boost de Velocidade")]
    [SerializeField] private float multiplicadorBoost = 2f;
    [SerializeField] private float duracaoBoost = 5f;
    private bool boostAtivo = false;
    private float velocidadeOriginal;
    private Coroutine corotinaBoost;
    
    [Header("Sistema de Objetivos da Fase")]
    [SerializeField] private int materiasNecessarias = 2;
    private int materiasColetadas = 0;
    
    [Header("Componentes")]
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    
    // Variáveis de controle
    private float proximoAtaque = 0f;
    private Vector2 movimentoInput;
    private Vector2 direcaoAtaque;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        // Se não tiver Animator atribuído, tenta pegar do componente
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        velocidadeAtual = velocidadeNormal;
        velocidadeOriginal = velocidadeNormal; // Guarda velocidade original
        
        // Configuração do Rigidbody2D para não atravessar paredes
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Impede rotação
        }
        
        // Verifica se tem collider
        if (playerCollider == null)
        {
            Debug.LogError("ATENÇÃO: Adicione um Collider2D (Circle ou Box) ao Player!");
        }
        else
        {
            Debug.Log("Player Collider detectado: " + playerCollider.GetType().Name);
        }
        
        if (rb == null)
        {
            Debug.LogError("ATENÇÃO: Adicione um Rigidbody2D ao Player!");
        }
        else
        {
            Debug.Log("Rigidbody2D configurado - Gravity: " + rb.gravityScale + " | BodyType: " + rb.bodyType);
        }
        
        if (animator == null)
        {
            Debug.LogWarning("AVISO: Animator não encontrado. Animações não serão reproduzidas.");
        }
        else
        {
            Debug.Log("Animator detectado e configurado!");
        }
        
        Debug.Log("Player iniciado!");
    }
    
    // Detecta quando colidiu com algo (para debug)
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player colidiu com: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
    }
    
    // Detecta enquanto está colidindo (para debug)
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paredes"))
        {
            // Pode adicionar efeitos sonoros ou visuais aqui
        }
    }
    
    // Detecta quando entra em um Trigger (chaves, portas, etc)
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(">>> TRIGGER DETECTADO: " + other.gameObject.name + " | Tag: " + other.tag);
        
        // Detecta se pegou uma chave
        if (other.CompareTag("key"))
        {
            PegarChave(other.gameObject);
        }
        
        // Detecta se está na porta
        if (other.CompareTag("porta"))
        {
            TentarAbrirPorta(other.gameObject);
        }
        
        // Detecta se pegou a bota (boost de velocidade)
        if (other.CompareTag("bota"))
        {
            PegarBota(other.gameObject);
        }
        
        // Detecta se pegou matéria
        if (other.CompareTag("materia"))
        {
            PegarMateria(other.gameObject);
        }
        
        // Detecta se pegou o trofeu (finalizar fase)
        if (other.CompareTag("trofeu"))
        {
            TentarPegarTrofeu(other.gameObject);
        }
    }

    void Update()
    {
        ProcessarInput();
        ProcessarAtaque();
        AtualizarAnimacoes();
    }
    
    void FixedUpdate()
    {
        Mover();
    }
    
    void ProcessarInput()
    {
        // Se não pode se mover, zera o input e retorna
        if (!isRunning)
        {
            movimentoInput = Vector2.zero;
            return;
        }
        
        // Captura input de movimento (WASD ou Setas)
        float horizontal = 0f;
        float vertical = 0f;
        
        // Horizontal (A/D ou Setas Esquerda/Direita)
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal = 1f;
        }
        
        // Vertical (W/S ou Setas Cima/Baixo)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vertical = 1f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vertical = -1f;
        }
        
        movimentoInput = new Vector2(horizontal, vertical).normalized;
        
        // Guarda a última direção de movimento para o ataque
        if (movimentoInput != Vector2.zero)
        {
            direcaoAtaque = movimentoInput;
        }
        
        // Verifica se está segurando Shift para andar devagar
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            velocidadeAtual = velocidadeLenta;
        }
        else
        {
            velocidadeAtual = velocidadeNormal;
        }
    }
    
    void Mover()
    {
        // Se não pode correr, para o personagem completamente
        if (!isRunning)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        
        // Move o personagem
        rb.velocity = movimentoInput * velocidadeAtual;
    }
    
    void ProcessarAtaque()
    {
        // Verifica se apertou SPACE e se pode atacar (respeitando o delay)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= proximoAtaque)
        {
            Atacar();
            proximoAtaque = Time.time + delayAtaque;
        }
    }
    
    void Atacar()
    {
        Debug.Log("Atacando!");
        
        // Inicia a animação de ataque
        if (animator != null)
        {
            animator.SetBool("isAttacking?", true);
        }
        
        // Detecta inimigos no range do ataque
        DetectarInimigosNoAtaque();
        
        // Para a animação depois do delay do ataque
        StartCoroutine(FinalizarAtaque());
    }
    
    void DetectarInimigosNoAtaque()
    {
        // Calcula a posição do ataque baseado na direção que o player está olhando
        Vector2 posicaoAtaque = (Vector2)transform.position + direcaoAtaque * (rangeAtaque / 2);
        
        // Detecta todos os colliders no range do ataque
        Collider2D[] objetosAtingidos = Physics2D.OverlapCircleAll(posicaoAtaque, rangeAtaque / 2);
        
        // Procura por inimigos
        foreach (Collider2D obj in objetosAtingidos)
        {
            if (obj.CompareTag("enemy"))
            {
                Debug.Log("Atingiu inimigo: " + obj.name);
                
                // Causa dano no inimigo
                EnemyAI enemyScript = obj.GetComponent<EnemyAI>();
                if (enemyScript != null)
                {
                    enemyScript.ReceberDano(danoAtaque);
                }
            }
        }
    }
    
    IEnumerator FinalizarAtaque()
    {
        // Espera o tempo do delay do ataque
        yield return new WaitForSeconds(delayAtaque);
        
        // Para a animação de ataque
        if (animator != null)
        {
            animator.SetBool("isAttacking?", false);
        }
    }
    
    // ======================== SISTEMA DE ANIMAÇÕES ========================
    void AtualizarAnimacoes()
    {
        // Se não tiver Animator, não faz nada
        if (animator == null) return;
        
        // Se está se movendo (tem input e pode correr)
        if (movimentoInput.magnitude > 0 && isRunning)
        {
            animator.SetBool("isRunning?", true);
        }
        else
        {
            animator.SetBool("isRunning?", false);
        }
        
        // NOTA: Direção do movimento (descomente se usar animações direcionais)
        // Se você tiver os parâmetros "Horizontal" e "Vertical" no Animator, descomente:
        // animator.SetFloat("Horizontal", movimentoInput.x);
        // animator.SetFloat("Vertical", movimentoInput.y);
        
        // Faz o flip quando anda para os lados
        FazerFlip();
    }
    
    void FazerFlip()
    {
        // Se está se movendo horizontalmente
        if (movimentoInput.x != 0)
        {
            // Se está indo para a esquerda (negativo)
            if (movimentoInput.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1); // Flip para esquerda
            }
            // Se está indo para a direita (positivo)
            else if (movimentoInput.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1); // Flip para direita (normal)
            }
        }
    }
    
    // Métodos para controlar o movimento do player
    public void PararMovimento()
    {
        isRunning = false;
        Debug.Log("Player parado - movimento desabilitado");
    }
    
    public void LiberarMovimento()
    {
        isRunning = true;
        Debug.Log("Player liberado - movimento habilitado");
    }
    
    public void SetMovimento(bool podeCorrer)
    {
        isRunning = podeCorrer;
        Debug.Log("Movimento do player: " + (isRunning ? "HABILITADO" : "DESABILITADO"));
    }
    
    public bool EstaCorrente()
    {
        return isRunning;
    }
    
    // ======================== SISTEMA DE CHAVES E PORTAS ========================
    void PegarChave(GameObject chaveObj)
    {
        // Proteção: verifica se o objeto ainda existe
        if (chaveObj == null) return;
        
        chaves++;
        Debug.Log("=== CHAVE COLETADA ===");
        Debug.Log("Objeto: " + chaveObj.name);
        Debug.Log("Tag: " + chaveObj.tag);
        Debug.Log("Total de chaves: " + chaves);
        Debug.Log("=====================");
        
        // Desativa o objeto imediatamente (evita coletar 2x)
        chaveObj.SetActive(false);
        
        // Muda a tag antes de destruir
        chaveObj.tag = "Untagged";
        
        // Destrói o objeto com um pequeno delay (evita bugs de hierarquia)
        Destroy(chaveObj, 0.1f);
        
        // Aqui você pode adicionar efeitos sonoros, animação, etc.
        // Exemplo: AudioSource.PlayClipAtPoint(somChave, transform.position);
    }
    
    void TentarAbrirPorta(GameObject portaObj)
    {
        // Pega o script DoorController da porta
        DoorController doorController = portaObj.GetComponent<DoorController>();
        
        // Verifica se a porta já foi aberta
        if (portaObj.CompareTag("porta_aberta"))
        {
            Debug.Log("Esta porta já está aberta!");
            return;
        }
        
        Debug.Log("Tentando abrir porta: " + portaObj.name + " | Chaves: " + chaves);
        
        // Verifica se tem pelo menos 1 chave
        if (chaves > 0)
        {
            // Usa uma chave
            chaves--;
            Debug.Log("Porta aberta! Chaves restantes: " + chaves);
            
            // Se a porta tem o DoorController, usa ele (com animação)
            if (doorController != null)
            {
                doorController.AbrirPorta();
            }
            else
            {
                // FALLBACK: Se não tem DoorController, usa o método antigo
                Debug.LogWarning("DoorController não encontrado! Abrindo porta sem animação.");
                
                portaObj.tag = "porta_aberta";
                
                Collider2D[] colliders = portaObj.GetComponents<Collider2D>();
                foreach (Collider2D collider in colliders)
                {
                    if (!collider.isTrigger)
                    {
                        collider.enabled = false;
                        Debug.Log("Collider sólido desativado - passagem liberada!");
                    }
                }
            }
        }
        else
        {
            // Não tem chave
            Debug.Log("Porta trancada! Você precisa de uma chave.");
            
            // Se tem DoorController, chama o método de porta trancada
            if (doorController != null)
            {
                doorController.PortaTrancada();
            }
        }
    }
    
    // Getters para acessar as chaves
    public int GetChaves()
    {
        return chaves;
    }
    
    public void AdicionarChave(int quantidade = 1)
    {
        chaves += quantidade;
        Debug.Log("Chaves adicionadas! Total: " + chaves);
    }
    
    // ======================== SISTEMA DE BOOST DE VELOCIDADE ========================
    void PegarBota(GameObject botaObj)
    {
        // Proteção: verifica se o objeto ainda existe
        if (botaObj == null) return;
        
        Debug.Log("=== BOTA COLETADA ===");
        Debug.Log("Objeto: " + botaObj.name);
        Debug.Log("Tag: " + botaObj.tag);
        Debug.Log("Velocidade 2x por 5 segundos!");
        Debug.Log("=====================");
        
        // Desativa o objeto imediatamente (evita coletar 2x)
        botaObj.SetActive(false);
        
        // Muda a tag antes de destruir
        botaObj.tag = "Untagged";
        
        // Destrói o objeto com um pequeno delay (evita bugs de hierarquia)
        Destroy(botaObj, 0.1f);
        
        // Para a corrotina anterior se existir
        if (corotinaBoost != null)
        {
            StopCoroutine(corotinaBoost);
            Debug.Log("Boost anterior cancelado, reiniciando timer...");
        }
        
        // Inicia novo boost
        corotinaBoost = StartCoroutine(AtivarBoost());
    }
    
    IEnumerator AtivarBoost()
    {
        boostAtivo = true;
        
        // Aumenta a velocidade
        velocidadeNormal = velocidadeOriginal * multiplicadorBoost;
        
        Debug.Log("BOOST ATIVO! Velocidade: " + velocidadeNormal);
        
        // Espera a duração do boost
        yield return new WaitForSeconds(duracaoBoost);
        
        // Restaura a velocidade normal
        velocidadeNormal = velocidadeOriginal;
        boostAtivo = false;
        corotinaBoost = null;
        
        Debug.Log("Boost acabou! Velocidade normal: " + velocidadeNormal);
    }
    
    // ======================== SISTEMA DE OBJETIVOS DA FASE ========================
    void PegarMateria(GameObject materiaObj)
    {
        // Proteção: verifica se o objeto ainda existe
        if (materiaObj == null) return;
        
        materiasColetadas++;
        Debug.Log("=== MATÉRIA COLETADA ===");
        Debug.Log("Objeto: " + materiaObj.name);
        Debug.Log("Tag: " + materiaObj.tag);
        Debug.Log("Total: " + materiasColetadas + "/" + materiasNecessarias);
        Debug.Log("========================");
        
        // Desativa o objeto imediatamente (evita coletar 2x)
        materiaObj.SetActive(false);
        
        // Muda a tag antes de destruir
        materiaObj.tag = "Untagged";
        
        // Destrói o objeto com um pequeno delay (evita bugs de hierarquia)
        Destroy(materiaObj, 0.1f);
        
        // Verifica se coletou todas
        if (materiasColetadas >= materiasNecessarias)
        {
            Debug.Log("Todas as matérias coletadas! Agora você pode pegar o trofeu!");
        }
    }
    
    void TentarPegarTrofeu(GameObject trofeuObj)
    {
        // NOTA: A lógica de carregar a próxima fase agora está no TrofeuController.cs
        // Este método só existe para manter compatibilidade, mas o TrofeuController
        // faz toda a verificação e carregamento automaticamente
        
        // Você pode deixar este método vazio ou adicionar efeitos visuais/sonoros aqui
        Debug.Log("Player tocou no trofeu - O TrofeuController vai cuidar do resto!");
    }
    
    // Getters para UI
    public int GetMateriasColetadas()
    {
        return materiasColetadas;
    }
    
    public int GetMateriasNecessarias()
    {
        return materiasNecessarias;
    }
    
    public bool BoostAtivo()
    {
        return boostAtivo;
    }
    
    // Visualização do range de ataque no Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 posicaoAtaque = (Vector2)transform.position + direcaoAtaque * (rangeAtaque / 2);
        Gizmos.DrawWireSphere(posicaoAtaque, rangeAtaque / 2);
    }
}
