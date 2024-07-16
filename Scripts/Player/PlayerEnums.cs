// 현재 플레이어의 상태가 무엇인지 ?
public enum PlayerState
{
    Alive,
    Die,
}

// 플레이어가 현재 총을 쏠 수 있는 상태인지 ?
public enum PlayerShootState
{
    Shooting,       // 쏘는 중
    Reloading,      // 장전 중
    NotShooting,    // 쏘지 않는 중
    NoBullets,      // 총알 없음
}
