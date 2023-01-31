using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Sword, Range, Guard, Wizard, Bullet };
public enum Condition { Move, Attack, Hit, Die, Stop};

public class Unit : MonoBehaviour
{
	public UnitType type; //유닛의 타입
	public int speed; 
	public ParticleSystem Dust;

	private float distance; //레이저 길이
	LayerMask enemy; //적군의 레이어
	LayerMask ally; //아군의 레이어
	RaycastHit2D hit; 
	RaycastHit2D wait;
	Animator anim;
	Vector3 direction; //적군 탐지 레이저를 쏠 방향
	Vector3 allyDirection; // 아군 탐지 레이저를 쏠 방향
	private Condition con; //유닛의 현 상태
	private bool enemyScan = true;
	BoxCollider2D boxCol;
	SpriteRenderer ren;

	float Hp;
	float Attack;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		boxCol = GetComponent<BoxCollider2D>();
		ren = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		//유닛 타입 별로 필요한 값 설정
		TypeSet();
	}

	private void FixedUpdate()
	{
		doMove();
	}

	private void doMove()
	{
		if (con == Condition.Move) //이동 상태일 때 더스트 파티클 실행 및 이동
		{
			if (enemyScan) // 적군 탐색 값이 true 일때
			{
				hit = Physics2D.Raycast(transform.position, direction, distance, enemy); //적만 탐색하는 레이저 
				Debug.DrawRay(transform.position, direction, Color.green, 0f);
				if (hit)
				{
					con = Condition.Attack; //상태 Attack으로 바꿈
					doAttack(); //공격
				}
				else // 탐색 실패시 적군 탐색 false
					enemyScan = false;
			}
			else //적군 탐색 false일 때
			{
				wait = Physics2D.Raycast(transform.position, allyDirection, 0.3f, ally);//아군 탐색 레이저는 유닛 타입에 따라 변화하지 않음
				Debug.DrawRay(transform.position, allyDirection, Color.blue, 0f);
				if (wait)
				{
					con = Condition.Stop;//탐색 되면 Stop실행
					doStop();
				}
				else //아군 탐색 실패시 적군 탐색 true
					enemyScan = true;
			}
			if (!(Dust == null))
				if(!Dust.isPlaying)
					Dust.Play();
			transform.position = new Vector3(transform.position.x + speed * Time.deltaTime, 0f, 0f);// 이동
		}
		else//이동 상태 아닐땐 파티클 실행 중지
		{
			if (!(Dust == null)) //null이랑 .isPlaying 체크해야 버그없음
				if(Dust.isPlaying)
					Dust.Stop();
		}
	}

	private void TypeSet()
	{
		switch (type) // 유닛 타입별로 적군 탐색 레이저 길이 조정
		{
			case UnitType.Sword:
				Hp = 10f;
				Attack = 2f;
				distance = 0.7f;
				break;
			case UnitType.Range:
				Hp = 10f;
				Attack = 1f;
				distance = 2.5f;
				break;
			case UnitType.Guard:
				Hp = 10f;
				Attack = 1f;
				distance = 0.7f;
				break;
			case UnitType.Wizard:
				Hp = 10f;
				Attack = 1f;
				distance = 3.5f;
				break;
			case UnitType.Bullet:
				Hp = 10f;
				Attack = 1f;
				distance = 0.4f;
				break;
		}
		//각 진영에 따라 빔의 방향 설정
		if (gameObject.layer == 8)
		{
			ally = LayerMask.GetMask("Blue");
			enemy = LayerMask.GetMask("Red");
			direction = Vector3.right * distance;
			allyDirection = Vector3.right * 0.3f;
		}
		else if (gameObject.layer == 9)
		{
			ally = LayerMask.GetMask("Red");
			enemy = LayerMask.GetMask("Blue");
			direction = Vector3.left * distance;
			allyDirection = Vector3.left * 0.3f;
		}

		// 첫 모션은 이동으로 설정
		con = Condition.Move;
		anim.SetBool("doMove", true);
	}

	private void doAttack()
	{
		if (con == Condition.Attack)
		{
			anim.SetBool("doAttack", true);
			StartCoroutine(AttackDelay());
		}
	}

	IEnumerator AttackDelay()
	{
		hit.collider.GetComponent<Unit>().TakeDamage(Attack); //데미지 주기
		yield return new WaitForSeconds(Random.Range(0.8f, 1.2f));
		con = Condition.Move;
		anim.SetBool("doMove", true);
	}

	private void doStop()
	{
		if (con == Condition.Stop)
		{
			StartCoroutine(Delay(2f));
		}

	}
	IEnumerator Delay(float delay)
	{
		yield return new WaitForSeconds(delay);
		con = Condition.Move;
		anim.SetBool("doMove", true);
	}

	public void TakeDamage(float Attack)
	{
		StopAllCoroutines(); // 피격 시 이미 실행중인 다른 딜레이들과 겹쳐서 버그가 나지 않도록 모든 코루틴을 종료하고 시작한다.
		Hp -= Attack;
		if (Hp > 0)
		{
			con = Condition.Hit;
			anim.SetBool("doHit", true);
			StartCoroutine(Delay(0.8f));
		}
		else
		{
			con = Condition.Die;
			anim.SetBool("doDie", true);
			boxCol.enabled = false;
			ren.sortingOrder = 1;
			Destroy(gameObject,3f); //this로 하니까 스크립트만 깨지고 오브젝트는 남아있었음....
		}
	}
}
