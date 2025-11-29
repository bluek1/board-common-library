import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { postsApi } from '../api';
import { Post, PostStatus } from '../types';

export function PostListPage() {
  const [posts, setPosts] = useState<Post[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');

  const categories = ['전체', '공지사항', '자유게시판', '질문게시판', '정보공유', '잡담'];

  useEffect(() => {
    loadPosts();
  }, [page, category]);

  const loadPosts = async () => {
    setIsLoading(true);
    setError('');
    try {
      const result = await postsApi.getAll({
        page,
        pageSize: 10,
        search: search || undefined,
        category: category && category !== '전체' ? category : undefined,
      });
      setPosts(result.items);
      setTotalPages(result.totalPages);
    } catch (err: any) {
      setError('게시물을 불러오는데 실패했습니다.');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    loadPosts();
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('ko-KR', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="space-y-6">
      {/* 헤더 */}
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">게시판</h1>
        <Link
          to="/posts/new"
          className="px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition"
        >
          새 글 작성
        </Link>
      </div>

      {/* 검색 & 필터 */}
      <div className="bg-white p-4 rounded-lg shadow-sm space-y-4">
        <form onSubmit={handleSearch} className="flex gap-2">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="검색어를 입력하세요..."
            className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <button
            type="submit"
            className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition"
          >
            검색
          </button>
        </form>

        <div className="flex gap-2 flex-wrap">
          {categories.map((cat) => (
            <button
              key={cat}
              onClick={() => {
                setCategory(cat === '전체' ? '' : cat);
                setPage(1);
              }}
              className={`px-3 py-1 rounded-full text-sm font-medium transition ${
                (cat === '전체' && !category) || cat === category
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {cat}
            </button>
          ))}
        </div>
      </div>

      {/* 게시물 목록 */}
      {isLoading ? (
        <div className="text-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        </div>
      ) : error ? (
        <div className="text-center py-12 text-red-600">{error}</div>
      ) : posts.length === 0 ? (
        <div className="text-center py-12 text-gray-500">게시물이 없습니다.</div>
      ) : (
        <div className="bg-white rounded-lg shadow-sm divide-y">
          {posts.map((post) => (
            <Link
              key={post.id}
              to={`/posts/${post.id}`}
              className="block p-4 hover:bg-gray-50 transition"
            >
              <div className="flex items-start justify-between">
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    {post.isPinned && (
                      <span className="px-2 py-0.5 bg-red-100 text-red-600 text-xs font-medium rounded">
                        📌 공지
                      </span>
                    )}
                    {post.category && (
                      <span className="px-2 py-0.5 bg-gray-100 text-gray-600 text-xs font-medium rounded">
                        {post.category}
                      </span>
                    )}
                    <h3 className="font-medium text-gray-900 truncate">{post.title}</h3>
                  </div>
                  <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span>{post.authorName}</span>
                    <span>{formatDate(post.createdAt)}</span>
                    <span>조회 {post.viewCount}</span>
                    {post.commentCount > 0 && (
                      <span className="text-blue-600">💬 {post.commentCount}</span>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-500 ml-4">
                  <span>❤️ {post.likeCount}</span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}

      {/* 페이지네이션 */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2">
          <button
            onClick={() => setPage(Math.max(1, page - 1))}
            disabled={page === 1}
            className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50"
          >
            이전
          </button>
          {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
            const pageNum = Math.max(1, page - 2) + i;
            if (pageNum > totalPages) return null;
            return (
              <button
                key={pageNum}
                onClick={() => setPage(pageNum)}
                className={`px-3 py-1 rounded ${
                  page === pageNum
                    ? 'bg-blue-600 text-white'
                    : 'border border-gray-300 hover:bg-gray-50'
                }`}
              >
                {pageNum}
              </button>
            );
          })}
          <button
            onClick={() => setPage(Math.min(totalPages, page + 1))}
            disabled={page === totalPages}
            className="px-3 py-1 rounded border border-gray-300 disabled:opacity-50"
          >
            다음
          </button>
        </div>
      )}
    </div>
  );
}
