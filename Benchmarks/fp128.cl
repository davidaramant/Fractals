// Fixed-point 128 bits functions
// (c) EB Dec 2009

// We store signed 128-bit integers on one uint4.
// The associated real value is X/2^96 (ie first
// int is the integer part).

// Increment U
uint4 inc128(uint4 u)
{
    // Compute all carries to add
    int4 h = (u == (uint4)(0xFFFFFFFF)); // Note that == sets ALL bits if true (�6.3.d)
    uint4 c = (uint4)(h.y&h.z&h.w & 1, h.z&h.w & 1, h.w & 1, 1);
    return u + c;
}

// Return -U
uint4 neg128(uint4 u)
{
    return inc128(u ^ (uint4)(0xFFFFFFFF)); // 1 + not U
}

// Return representation of signed integer K
uint4 set128(int k)
{
    uint4 u = (uint4)((uint)abs(k), 0, 0, 0);
    return (k < 0) ? neg128(u) : u;
}

// Return U+V
uint4 add128(uint4 u, uint4 v)
{
    uint4 s = u + v;
    uint4 h = (uint4)(s < u);
    uint4 c1 = h.yzwx & (uint4)(1, 1, 1, 0); // Carry from U+V
    h = (uint4)(s == (uint4)(0xFFFFFFFF));
    uint4 c2 = (uint4)((c1.y | (c1.z&h.z))&h.y, c1.z&h.z, 0, 0); // Propagated carry
    return s + c1 + c2;
}

// Return U<<1
uint4 shl128(uint4 u)
{
    uint4 h = (u >> (uint4)(31U)) & (uint4)(0, 1, 1, 1); // Bits to move up
    return (u << (uint4)(1U)) | h.yzwx;
}

// Return U>>1
uint4 shr128(uint4 u)
{
    uint4 h = (u << (uint4)(31U)) & (uint4)(0x80000000U, 0x80000000U, 0x80000000U, 0); // Bits to move down
    return (u >> (uint4)(1U)) | h.wxyz;
}

// Return U*K.
// U MUST be positive.
uint4 mul128u(uint4 u, uint k)
{
    uint4 s1 = u * (uint4)(k);
    uint4 s2 = (uint4)(mul_hi(u.y, k), mul_hi(u.z, k), mul_hi(u.w, k), 0);
    return add128(s1, s2);
}

// Return U*K, handles signs (K != INT_MIN).
uint4 mul128(uint4 u, int k)
{
    // Sign bits
    uint su = u.x & 0x80000000U;
    uint sk = (uint)k & 0x80000000U;
    // Abs values
    uint4 uu = (su) ? neg128(u) : u;
    uint kk = (uint)((sk) ? (-k) : k);
    // Product
    uint4 p = mul128u(uu, kk);
    // Product sign
    return (su^sk) ? neg128(p) : p;
}

// Return U*V truncated to keep the position of the decimal point.
// U and V MUST be positive.
uint4 mulfpu(uint4 u, uint4 v)
{
    // Diagonal coefficients
    uint4 s = (uint4)(u.x*v.x, mul_hi(u.y, v.y), u.y*v.y, mul_hi(u.z, v.z));
    // Off-diagonal
    uint4 t1 = (uint4)(mul_hi(u.x, v.y), u.x*v.y, mul_hi(u.x, v.w), u.x*v.w);
    uint4 t2 = (uint4)(mul_hi(v.x, u.y), v.x*u.y, mul_hi(v.x, u.w), v.x*u.w);
    s = add128(s, add128(t1, t2));
    t1 = (uint4)(0, mul_hi(u.x, v.z), u.x*v.z, mul_hi(u.y, v.w));
    t2 = (uint4)(0, mul_hi(v.x, u.z), v.x*u.z, mul_hi(v.y, u.w));
    s = add128(s, add128(t1, t2));
    t1 = (uint4)(0, 0, mul_hi(u.y, v.z), u.y*v.z);
    t2 = (uint4)(0, 0, mul_hi(v.y, u.z), v.y*u.z);
    s = add128(s, add128(t1, t2));
    // Add 3 to compensate for the truncation
    return add128(s, (uint4)(0, 0, 0, 3));
}

// Return U*U truncated to keep the position of the decimal point.
// U MUST be positive.
uint4 sqrfpu(uint4 u)
{
    // Diagonal coefficients
    uint4 s = (uint4)(u.x*u.x, mul_hi(u.y, u.y), u.y*u.y, mul_hi(u.z, u.z));
    // Off-diagonal
    uint4 t = (uint4)(mul_hi(u.x, u.y), u.x*u.y, mul_hi(u.x, u.w), u.x*u.w);
    s = add128(s, shl128(t));
    t = (uint4)(0, mul_hi(u.x, u.z), u.x*u.z, mul_hi(u.y, u.w));
    s = add128(s, shl128(t));
    t = (uint4)(0, 0, mul_hi(u.y, u.z), u.y*u.z);
    s = add128(s, shl128(t));
    // Add 3 to compensate for the truncation
    return add128(s, (uint4)(0, 0, 0, 3));
}

// Return U*V, handles signs
uint4 mulfp(uint4 u, uint4 v)
{
    // Sign bits
    uint su = u.x & 0x80000000U;
    uint sv = v.x & 0x80000000U;
    // Abs values
    uint4 uu = (su) ? neg128(u) : u;
    uint4 vv = (sv) ? neg128(v) : v;
    // Product
    uint4 p = mulfpu(uu, vv);
    // Product sign
    return (su^sv) ? neg128(p) : p;
}

// Return U*U, handles signs
uint4 sqrfp(uint4 u)
{
    // Sign bit
    uint su = u.x & 0x80000000U;
    // Abs value
    uint4 uu = (su) ? neg128(u) : u;
    // Product is positive
    return sqrfpu(uu);
}

#ifdef UNIT_TESTS

// Perform some tests
__kernel void test_fp128(__global uint * a, uint n)
{
    uint4 u = 0U;
    uint4 v = 0U;
    uint4 sum = 0U; // accumulates all error bits, should be near 0 on exit
                    // clear buffer
    vstore4(sum, 0, a);
    vstore4(sum, 1, a);
    vstore4(sum, 2, a);
    vstore4(sum, 3, a);

#if 0
    // neg128
    u = (uint4)(2, 0x80000000, 0xFFFFFFFF, 0xFFFFFF00);
    for (int i = 0; i < n; i++)
    {
        v = neg128(neg128(u)) ^ u;
    }
    sum |= v;
#endif
#if 0
    // add128
    u = (uint4)(0, 0x01000000, 0x00000100, 0x00010000);
    v = (uint4)(0xFFFFFFFE, 0xFFFFF000, 0xFFFFFFFF, 0x100FFFFF);
    uint4 v0 = v;
    for (int i = 0; i < n; i++)
    {
        v = add128(v, u);
    }
    u = neg128(u);
    for (int i = 0; i < n; i++)
    {
        v = add128(v, u);
    }
    sum |= (v^v0);
#endif
#if 0
    // add128 bugs
    u = (uint4)(0x0000ABCD, 0xEF000000, 0xABCDEFAB, 0xCDEF0000);
    sum |= add128(u, neg128(u));
#endif
#if 1
    // mul128u
    u = (uint4)(0x00000000, 0x01000000, 0x00000100, 0x00010000);
    for (int i = 0; i < n; i++)
    {
        v = mul128u(u, 0x00ABCDEF);
    }
    vstore4(u, 1, a);
    u = (uint4)(0x0000ABCD, 0xEF000000, 0xABCDEFAB, 0xCDEF0000);
    sum |= (v^u);
#endif
#if 0
    // mulfp and sqrfp
    uint4 sqrt2 = (uint4)(1, 0x6A09E667, 0xF3BCC908, 0xB2FB1366); // Sqrt(2) in our representation
    uint4 rsqrt2 = shr128(sqrt2); // 1/Sqrt(2)
    uint4 minus2 = set128(-2); // -2.0
    uint4 minus1 = set128(-1); // -2.0
    for (int i = 0; i < n; i++)
    {
        v = mulfp(sqrt2, sqrt2);
        v = add128(v, minus2); // near 0
        if (v.x & 0x80000000) v = neg128(v); // abs(v)
    }
    sum |= v;
    for (int i = 0; i < n; i++)
    {
        v = sqrfp(sqrt2);
        v = add128(v, minus2); // near 0
        if (v.x & 0x80000000) v = neg128(v); // abs(v)
    }
    sum |= v;
    for (int i = 0; i < n; i++)
    {
        v = mulfp(sqrt2, rsqrt2);
        v = add128(v, minus1); // near 0
        if (v.x & 0x80000000) v = neg128(v); // abs(v)
    }
    vstore4(mulfp(sqrt2, rsqrt2), 1, a);
    vstore4(rsqrt2, 2, a);
    vstore4(sqrt2, 3, a);
    sum |= v;
#endif
    vstore4(sum, 0, a); // Stores all error bits in the first 4 words of A
}

#endif // UNIT_TESTS

