# Random Walk Lattice Cipher

Pretty simple to use, only a few functions. Relies on passwords and a secure hashing algorithm of your choice (SHA-256 is used in this example), it could also be stretched using Bcrypt or Argon2.

## Details

- Takes place on an N-Dimensional lattice
- Using a random number generator seeded by a password, a point on this lattice is randomly chosen for each character input that is dependant on that last random point
- The sum of that points vector components are taken, and the character is then xor'd using the same sum.
- decryption happens in reverse
